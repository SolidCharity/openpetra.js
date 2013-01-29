//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       christiank, timop
//
// Copyright 2004-2013 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Collections;
using System.Threading;
using System.Security.Cryptography;
using System.Security.Principal;
using Ict.Common;
using Ict.Common.Remoting.Server;
using Ict.Common.Remoting.Shared;

namespace Ict.Common.Remoting.Server
{
    /// <summary>
    /// collection of static functions and variables for the appdomain management
    /// </summary>
    public class DomainManager
    {
        // TODORemoting those static variables need to be resolved

        /// <summary>used internally to store the ClientID for which this AppDomain was created</summary>
        public static Int32 GClientID;

        /// <summary>used internally to hold SiteKey Information (for convenience)</summary>
        public static Int64 GSiteKey;

        /// <summary>tells when the last remoteable object was marshaled (remoted).</summary>
        public static DateTime ULastObjectRemotingAction;

        /// <summary>
        /// overload
        /// </summary>
        /// <param name="ATaskGroup"></param>
        /// <param name="ATaskCode"></param>
        /// <param name="ATaskPriority"></param>
        /// <returns></returns>
        public static Int32 ClientTaskAdd(String ATaskGroup, String ATaskCode, Int16 ATaskPriority)
        {
            return ClientTaskAdd(ATaskGroup, ATaskCode, null, null, null, null, ATaskPriority);
        }

        /// <summary>
        /// add a task for the client
        /// </summary>
        /// <param name="ATaskGroup"></param>
        /// <param name="ATaskCode"></param>
        /// <param name="ATaskParameter1"></param>
        /// <param name="ATaskParameter2"></param>
        /// <param name="ATaskParameter3"></param>
        /// <param name="ATaskParameter4"></param>
        /// <param name="ATaskPriority"></param>
        /// <returns></returns>
        public static Int32 ClientTaskAdd(String ATaskGroup,
            String ATaskCode,
            System.Object ATaskParameter1,
            System.Object ATaskParameter2,
            System.Object ATaskParameter3,
            System.Object ATaskParameter4,
            Int16 ATaskPriority)
        {
            return TClientManager.QueueClientTask(GClientID,
                ATaskGroup,
                ATaskCode,
                ATaskParameter1,
                ATaskParameter2,
                ATaskParameter3,
                ATaskParameter4,
                ATaskPriority);
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="AClientID"></param>
        /// <param name="ATaskGroup"></param>
        /// <param name="ATaskCode"></param>
        /// <param name="ATaskParameter1"></param>
        /// <param name="ATaskParameter2"></param>
        /// <param name="ATaskParameter3"></param>
        /// <param name="ATaskParameter4"></param>
        /// <param name="ATaskPriority"></param>
        /// <returns></returns>
        public static Int32 ClientTaskAddToOtherClient(Int16 AClientID,
            String ATaskGroup,
            String ATaskCode,
            object ATaskParameter1,
            object ATaskParameter2,
            object ATaskParameter3,
            object ATaskParameter4,
            Int16 ATaskPriority)
        {
            return TClientManager.QueueClientTask(AClientID,
                ATaskGroup,
                ATaskCode,
                ATaskParameter1,
                ATaskParameter2,
                ATaskParameter3,
                ATaskParameter4,
                ATaskPriority,
                GClientID);
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="AUserID"></param>
        /// <param name="ATaskGroup"></param>
        /// <param name="ATaskCode"></param>
        /// <param name="ATaskParameter1"></param>
        /// <param name="ATaskParameter2"></param>
        /// <param name="ATaskParameter3"></param>
        /// <param name="ATaskParameter4"></param>
        /// <param name="ATaskPriority"></param>
        /// <returns></returns>
        public static Int32 ClientTaskAddToOtherClient(String AUserID,
            String ATaskGroup,
            String ATaskCode,
            object ATaskParameter1,
            object ATaskParameter2,
            object ATaskParameter3,
            object ATaskParameter4,
            Int16 ATaskPriority)
        {
            return TClientManager.QueueClientTask(AUserID,
                ATaskGroup,
                ATaskCode,
                ATaskParameter1,
                ATaskParameter2,
                ATaskParameter3,
                ATaskParameter4,
                ATaskPriority,
                GClientID);
        }
    }

    /// <summary>
    /// Sets up .NET Remoting for this AppDomain, remotes a simple object which
    /// can be called to from outside, sets up and remotes the Server-side .NET
    /// Remoting Sponsor and opens a DB connection for the AppDomain.
    ///
    /// It also monitors the Server-side .NET Remoting Sponsor and tears down the
    /// AppDomain if the Sponsor is disconnected from .NET Remoting, which only
    /// happens if the Client fails to call the Sponsor's KeepAlive method
    /// regularily (that means that either the Client crashed or the connection
    /// between the Client and the Server has broken). In either case the AppDomain
    /// and all objects that are instantiated in it are of no use anymore - the
    /// Client can only connect anew, and then a new AppDomain is created.
    ///
    /// @comment This class gets instantiated from TClientAppDomainConnection.
    ///
    /// @comment WARNING: The name of the class and the names of many functions and
    /// procedures must not be changed, because they get instantiated/invoked via
    /// .NET Reflection, that is 'late-bound'. If you need to rename them, you also
    /// need to change the Strings with their names in the .NET Reflection calls in
    /// TClientAppDomainConnection!
    ///
    /// </summary>
    public class TClientDomainManagerBase
    {
        /// <summary>UserID for which this AppDomain was created</summary>
        private String FUserID;

        /// <summary>ClientServer connection type</summary>
        private TClientServerConnectionType FClientServerConnectionType;

        /// <summary>holds reference to an instance of the ClientTasksManager</summary>
        private TClientTasksManager FClientTasksManager;

        private TPollClientTasks FRemotedPollClientTaskObject;

        /// <summary>Random Security Token (to prevent unauthorised AppDomain shutdown)</summary>
        private String FRandomAppDomainTearDownToken;
        private System.Object FTearDownAppDomainMonitor;

        /// <summary>Tells when the last Client Action occured (the last time when a remoteable object was marshaled (remoted)).
        /// Can be overloaded by a server with database access to see when the last DB action occured</summary>
        public virtual DateTime LastActionTime
        {
            get
            {
                return DomainManager.ULastObjectRemotingAction;
            }
        }


        /// <summary>
        /// Inserts a ClientTask into this Clients Task queue.
        ///
        /// @comment WARNING: If you need to rename this function or change its parameters,
        /// you also need to change the String with its name and the parameters in the
        /// .NET Reflection call in TClientAppDomainConnection!
        ///
        /// </summary>
        /// <param name="ATaskGroup">Group of the Task</param>
        /// <param name="ATaskCode">Code of the Task (depending on the TaskGroup this can be
        /// left empty)</param>
        /// <param name="ATaskParameter1">Parameter #1 for the Task (depending on the TaskGroup
        /// this can be left empty)</param>
        /// <param name="ATaskParameter2">Parameter #2 for the Task (depending on the TaskGroup
        /// this can be left empty)</param>
        /// <param name="ATaskParameter3">Parameter #3 for the Task (depending on the TaskGroup
        /// this can be left empty)</param>
        /// <param name="ATaskParameter4">Parameter #4 for the Task (depending on the TaskGroup
        /// this can be left empty)</param>
        /// <param name="ATaskPriority">Priority of the Task</param>
        /// <returns>TaskID
        /// </returns>
        public Int32 ClientTaskAdd(String ATaskGroup,
            String ATaskCode,
            System.Object ATaskParameter1,
            System.Object ATaskParameter2,
            System.Object ATaskParameter3,
            System.Object ATaskParameter4,
            Int16 ATaskPriority)
        {
            return FClientTasksManager.ClientTaskAdd(ATaskGroup,
                ATaskCode,
                ATaskParameter1,
                ATaskParameter2,
                ATaskParameter3,
                ATaskParameter4,
                ATaskPriority);
        }

        #region TClientDomainManager

        /// <summary>
        /// Sets up .NET Remoting Lifetime Services and TCP Channel for this AppDomain.
        ///
        /// @comment WARNING: If you need to change the parameters of the Constructor,
        /// you also need to change the parameters in the .NET Reflection call in
        /// TClientAppDomainConnection!
        ///
        /// </summary>
        /// <param name="AClientID">ClientID as assigned by the ClientManager</param>
        /// <param name="AClientServerConnectionType">Tells in which way the Client connected
        /// to the PetraServer</param>
        /// <param name="AUserID"></param>
        public TClientDomainManagerBase(Int32 AClientID,
            TClientServerConnectionType AClientServerConnectionType,
            string AUserID)
        {
            new TAppSettingsManager(false);

            FUserID = AUserID;

            // Console.WriteLine('TClientDomainManager.Create in AppDomain: ' + Thread.GetDomain().FriendlyName);
            DomainManager.GClientID = AClientID;
            FClientServerConnectionType = AClientServerConnectionType;
            FClientTasksManager = new TClientTasksManager();
            FTearDownAppDomainMonitor = new System.Object();
            Random random = new Random();
            FRandomAppDomainTearDownToken = random.Next(Int32.MinValue, Int32.MaxValue).ToString();

            // TODORemoting setup life time services

            if (TLogging.DL >= 4)
            {
                Console.WriteLine("Application domain: " + Thread.GetDomain().FriendlyName);
                Console.WriteLine("  for User: " + FUserID);
            }
        }

        /// <summary>
        /// stop the appdomain of the client
        /// </summary>
        public void StopClientAppDomain()
        {
            if (TLogging.DL >= 5)
            {
                TLogging.Log("TClientDomainManager.StopClientAppDomain: calling StopClientStillAliveCheckThread...",
                    TLoggingType.ToConsole | TLoggingType.ToLogfile);
            }

            ClientStillAliveCheck.TClientStillAliveCheck.StopClientStillAliveCheckThread();
        }

        /// <summary>
        /// Creates a new static instance of TSrvSetting for this AppDomain and takes
        /// over all settings from the static TSrvSetting object in the Default
        /// AppDomain.
        ///
        /// @comment WARNING: If you need to rename this procedure or change its parameters,
        /// you also need to change the String with its name and the parameters in the
        /// .NET Reflection call in TClientAppDomainConnection!
        ///
        /// @comment The static TSrvSetting object in the Default AppDomain is
        /// inaccessible in this AppDomain!
        ///
        /// </summary>
        /// <returns>void</returns>
        public void InitAppDomain(TSrvSetting ASettings)
        {
            // Console.WriteLine('TClientDomainManager.InitAppDomain in AppDomain: ' + Thread.GetDomain().FriendlyName);

            new TSrvSetting(ASettings);
            new TAppSettingsManager(TSrvSetting.ConfigurationFile);

            TLogging.DebugLevel = TAppSettingsManager.GetInt16("Server.DebugLevel", 0);
        }

        /// <summary>
        /// Calls a method in ClientManager to tear down the AppDomain where this Object
        /// is residing.
        ///
        /// @comment This procedure is designed to be executed through a callback from
        /// TClientStillAliveCheck.
        ///
        /// </summary>
        /// <param name="AToken">Security token (which is passed in the constructor of
        /// TClientStillAliveCheck) to prevent malicious calling of this function</param>
        /// <param name="AReason">String telling the reason why the AppDomain is being teared down
        /// </param>
        /// <returns>void</returns>
        public void TearDownAppDomain(String AToken, String AReason)
        {
            if (TSrvSetting.ClientAppDomainShutdownAfterKeepAliveTimeout)
            {
                /*
                 * Use a Monitor to prevent the following code beeing executed simultaneusly!
                 * (for some odd reason this would happen on .NET/Windows, but not on
                 * mono/Linux).
                 */
                if (Monitor.TryEnter(FTearDownAppDomainMonitor))
                {
                    Monitor.Enter(FTearDownAppDomainMonitor);

                    // prevent unauthorised tearing down of AppDomain by comparing security token
                    if (AToken == FRandomAppDomainTearDownToken)
                    {
                        if (TLogging.DL >= 9)
                        {
                            Console.WriteLine("TearDownAppDomain: Tearing down ClientDomain!!! Reason: " + AReason);
                        }

                        // The AppDomain and all the objects that are instantiated in it cease
                        // to exist after the following call!!!
                        // > No further code can be executed in the AppDomain after that!
                        TClientManager.DisconnectClient((short)DomainManager.GClientID, out AReason);
                        Monitor.Exit(FTearDownAppDomainMonitor);
                    }
                }
            }
        }

        /// <summary>
        /// Parameterises and remotes a TPollClientTasks Object that will be used by the
        /// Client to poll for ClientTasks.
        ///
        /// @comment WARNING: If you need to rename this function or change its parameters,
        /// you also need to change the String with its name and the parameters in the
        /// .NET Reflection call in TClientAppDomainConnection!
        ///
        /// @comment The Client needs to make calls to the TPollClientTasks Object
        /// in regular intervals. If the calls don't come anymore, the Client's
        /// AppDomain will be unloaded by a thread of TClientStillAliveCheck!
        ///
        /// </summary>
        /// <returns>The URL at which the remoted TPollClientTasks Object can be reached.
        /// </returns>
        public String CreateClientStillAliveCheck()
        {
            // Set Parameters for TPollClientTasks Class
            new TPollClientTasksParameters(FClientTasksManager);

            FRemotedPollClientTaskObject = new TPollClientTasks();

            // Start ClientStillAliveCheck Thread
            new ClientStillAliveCheck.TClientStillAliveCheck(FClientServerConnectionType, new TDelegateTearDownAppDomain(
                    TearDownAppDomain), FRandomAppDomainTearDownToken);

            if (TLogging.DL >= 5)
            {
                Console.WriteLine("TClientDomainManager.CreateClientStillAliveCheck: created TClientStillAliveCheck.");
            }

            return string.Empty;
        }

        /// <summary>
        /// get the object for remoting
        /// </summary>
        public TPollClientTasks GetPollClientTasksObject()
        {
            return FRemotedPollClientTaskObject;
        }

        /// <summary>
        /// Returns the current Status of a Task in the Clients Task queue.
        ///
        /// </summary>
        /// <param name="ATaskID">Task ID
        /// </param>
        /// <returns>void</returns>
        public String ClientTaskStatus(System.Int32 ATaskID)
        {
            return FClientTasksManager.ClientTaskStatus(ATaskID);
        }

        #endregion
    }
}