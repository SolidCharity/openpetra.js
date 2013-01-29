//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
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
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Data;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Ict.Common;
using Ict.Common.Data; // Implicit reference
using Ict.Common.DB;
using Ict.Common.IO;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Server;
using Ict.Petra.Server.App.Core;
using Ict.Petra.Server.App.Core.Security;
using Ict.Petra.Shared.Security;
using Ict.Petra.Shared.Interfaces.MFinance;
using Ict.Petra.Server.MFinance.AP.UIConnectors;
using Ict.Petra.Server.MConference.Applications;
using Ict.Common.Verification;
using Ict.Petra.Shared.MFinance.AP.Data;
using Jayrock.Json;
using Ict.Petra.Server.MPartner.Import;
using Ict.Petra.Shared;
using Ict.Petra.Server.App.Delegates;

namespace Ict.Petra.Server.App.WebService
{
/// <summary>
/// this publishes the SOAP web services of OpenPetra.org
/// </summary>
    [WebService(Namespace = "http://www.openpetra.org/webservices/SessionManager")]
    [ScriptService]
    public class TOpenPetraOrgSessionManager : System.Web.Services.WebService
    {
        /// <summary>
        /// constructor, which is called for each http request
        /// </summary>
        public TOpenPetraOrgSessionManager() : base()
        {
            if (TLogging.DebugLevel >= 4)
            {
                TLogging.Log(HttpContext.Current.Request.PathInfo);
            }

            TOpenPetraOrgSessionManager.Init();
        }

        /// <summary>
        /// initialise the server once for everyone
        /// </summary>
        public static bool Init()
        {
            if (TServerManager.TheServerManager == null)
            {
                string configfilename = string.Empty;

                // make sure the correct config file is used
                if (Environment.CommandLine.Contains("/appconfigfile="))
                {
                    // this happens when we use fastcgi-mono-server4
                    configfilename = Environment.CommandLine.Substring(
                        Environment.CommandLine.IndexOf("/appconfigfile=") + "/appconfigfile=".Length);

                    if (configfilename.IndexOf(" ") != -1)
                    {
                        configfilename = configfilename.Substring(0, configfilename.IndexOf(" "));
                    }
                }
                else
                {
                    // this is the normal behaviour when running with local http server
                    configfilename = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "web.config";
                }

                new TAppSettingsManager(configfilename);
                new TSrvSetting();
                new TLogging(TSrvSetting.ServerLogFile);
                TLogging.DebugLevel = TAppSettingsManager.GetInt16("Server.DebugLevel", 0);

                DBAccess.SetFunctionForRetrievingCurrentObjectFromWebSession(SetDatabaseForSession,
                    GetDatabaseFromSession);

                UserInfo.SetFunctionForRetrievingCurrentObjectFromWebSession(SetUserInfoForSession,
                    GetUserInfoFromSession);

                Catalog.Init();

                TServerManager.TheServerManager = new TServerManager();

                try
                {
                    TServerManager.TheCastedServerManager.EstablishDBConnection();

                    TSystemDefaultsCache.GSystemDefaultsCache = new TSystemDefaultsCache();
                    DomainManager.GSiteKey = TSystemDefaultsCache.GSystemDefaultsCache.GetInt64Default(
                        Ict.Petra.Shared.SharedConstants.SYSDEFAULT_SITEKEY);

                    // initialise the cached tables
                    TSetupDelegates.Init();
                }
                catch (Exception e)
                {
                    TLogging.Log(e.Message);
                    TLogging.Log(e.StackTrace);
                    throw;
                }

                TLogging.Log("Server has been initialised");

                return true;
            }

            return false;
        }

        static readonly object _lockerClientID = new object();
        static Int32 FNextClientID = 1;

        private bool LoginInternal(string username, string password)
        {
            Int32 ProcessID;
            bool ASystemEnabled;

            try
            {
                TClientManager.PerformLoginChecks(
                    username.ToUpper(), password.Trim(), "WEB", "127.0.0.1", out ProcessID, out ASystemEnabled);
                Session["LoggedIn"] = true;

                DBAccess.GDBAccessObj.UserID = username.ToUpper();

                TServerManager.TheCastedServerManager.AddDBConnection(DBAccess.GDBAccessObj);

                Int32 ClientID;

                lock (_lockerClientID)
                {
                    ClientID = FNextClientID;
                    FNextClientID++;
                }

                Session["ClientDomainManager"] = new TClientDomainManager(
                    ClientID,
                    TClientServerConnectionType.csctRemote,
                    TSystemDefaultsCache.GSystemDefaultsCache,
                    TCacheableTablesManager.GCacheableTablesManager,
                    UserInfo.GUserInfo);

                return true;
            }
            catch (Exception e)
            {
                TLogging.Log(e.Message);
                TLogging.Log(e.StackTrace);
                Session["LoggedIn"] = false;
                Ict.Common.DB.DBAccess.GDBAccessObj.RollbackTransaction();
                DBAccess.GDBAccessObj.CloseDBConnection();
                return false;
            }
        }

        /// <summary>Login a user</summary>
        [WebMethod(EnableSession = true)]
        public bool Login(string username, string password)
        {
            bool loggedIn = LoginInternal(username, password);

            return loggedIn;
        }

        /// <summary>
        /// TODO: we should only use one database object per request, and not have global variables for database connections
        /// </summary>
        private static SortedList <string, TDataBase>FDatabaseObjects = new SortedList <string, TDataBase>();

        static private TDataBase GetDatabaseFromSession()
        {
            // if another thread gets called, then the session object is null
            if (HttpContext.Current == null)
            {
                if (Thread.CurrentThread.Name == null)
                {
                    throw new Exception(
                        "TOpenPetraOrgSessionManager.GetDatabaseFromSession: we do need a name for the thread for managing the database connection");
                }

                if (!FDatabaseObjects.ContainsKey(Thread.CurrentThread.Name))
                {
                    TDataBase db = new TDataBase();
                    FDatabaseObjects.Add(Thread.CurrentThread.Name, db);
                }

                return FDatabaseObjects[Thread.CurrentThread.Name];
            }

            if (HttpContext.Current.Session["DBAccessObj"] == null)
            {
                if (TServerManager.TheServerManager == null)
                {
                    TLogging.Log("GetDatabaseFromSession : TheServerManager is null");
                }
                else
                {
                    // disconnect web user after 2 minutes of inactivity. should disconnect itself already earlier
                    TServerManager.TheCastedServerManager.DisconnectTimedoutDatabaseConnections(2 * 60, "ANONYMOUS");

                    // disconnect normal users after 3 hours of inactivity
                    TServerManager.TheCastedServerManager.DisconnectTimedoutDatabaseConnections(3 * 60 * 60, "");

                    TServerManager.TheCastedServerManager.EstablishDBConnection();
                }
            }

            return (TDataBase)HttpContext.Current.Session["DBAccessObj"];
        }

        static private void SetDatabaseForSession(TDataBase database)
        {
            if (Thread.CurrentThread.Name == null)
            {
                // TLogging.Log("there is a new thread for session " + HttpContext.Current.Session.SessionID);
                System.Threading.Thread.CurrentThread.Name = "MainThread" + Guid.NewGuid().ToString();;
            }

            HttpContext.Current.Session["DBAccessObj"] = database;
        }

        static private TPetraPrincipal GetUserInfoFromSession()
        {
            return (TPetraPrincipal)HttpContext.Current.Session["UserInfo"];
        }

        static private void SetUserInfoForSession(TPetraPrincipal userinfo)
        {
            HttpContext.Current.Session["UserInfo"] = userinfo;
        }

        /// <summary>check if the user has logged in successfully</summary>
        [WebMethod(EnableSession = true)]
        public bool IsUserLoggedIn()
        {
            object loggedIn = Session["LoggedIn"];

            if ((null != loggedIn) && ((bool)loggedIn == true))
            {
                return true;
            }

            return false;
        }

        /// <summary>log the user out</summary>
        [WebMethod(EnableSession = true)]
        public bool Logout()
        {
            TLogging.Log("Logout from a session", TLoggingType.ToLogfile | TLoggingType.ToConsole);

            if (DBAccess.GDBAccessObj != null)
            {
                DBAccess.GDBAccessObj.CloseDBConnection();
            }

            // Session Abandon causes problems in Mono 2.10.x see https://bugzilla.novell.com/show_bug.cgi?id=669807
            // TODO Session.Abandon();
            Session.Clear();

            return true;
        }

        /// <summary>get user information</summary>
        [WebMethod(EnableSession = true)]
        public string GetUserInfo()
        {
            if (UserInfo.GUserInfo == null)
            {
                return THttpBinarySerializer.SerializeObject(false);
            }

            return THttpBinarySerializer.SerializeObject(UserInfo.GUserInfo, true);
        }

#if TODORemoting
        void ConnectClient(String AUserName,
            String APassword,
            String AClientComputerName,
            String AClientIPAddress,
            System.Version AClientExeVersion,
            TClientServerConnectionType AClientServerConnectionType,
            out String AClientName,
            out System.Int32 AClientID,
            out TExecutingOSEnum AServerOS,
            out Int32 AProcessID,
            out String AWelcomeMessage,
            out Boolean ASystemEnabled,
            out IPrincipal AUserInfo);

        /**
         * Calling DisconnectClient tears down the Client's AppDomain and therefore
         * kills all remoted objects for the Client that were not released before.
         *
         * See implementation of this class for more detailed description!
         *
         */
        Boolean DisconnectClient(System.Int32 AClientID, out String ACantDisconnectReason);


        /**
         * Calling DisconnectClient tears down the Client's AppDomain and therefore
         * kills all remoted objects for the Client that were not released before.
         *
         * See implementation of this class for more detailed description!
         *
         */
        Boolean DisconnectClient(System.Int32 AClientID, String AReason, out String ACantDisconnectReason);

        /**
         * Can be called to queue a ClientTask for a certain Client.
         *
         * See implementation of this class for more detailed description!
         *
         */
        Int32 QueueClientTaskFromClient(System.Int32 AClientID,
            String ATaskGroup,
            String ATaskCode,
            System.Int16 ATaskPriority,
            System.Int32 AExceptClientID = -1,
            object ATaskParameter1 = null,
            object ATaskParameter2 = null,
            object ATaskParameter3 = null,
            object ATaskParameter4 = null);

        /// <summary>
        /// add error to the log
        /// </summary>
        /// <param name="AErrorCode"></param>
        /// <param name="AContext"></param>
        /// <param name="AMessageLine1"></param>
        /// <param name="AMessageLine2"></param>
        /// <param name="AMessageLine3"></param>
        /// <param name="AUserID"></param>
        /// <param name="AProcessID"></param>
        void AddErrorLogEntry(String AErrorCode,
            String AContext,
            String AMessageLine1,
            String AMessageLine2,
            String AMessageLine3,
            String AUserID,
            Int32 AProcessID);


        /**
         * The following functions are only for development purposes (note that these
         * functions can also be invoked directly from the Server's menu)
         *
         */
        System.Int32 GCGetGCGeneration(object AInspectObject);

        /// <summary>
        /// perform garbage collection
        /// </summary>
        /// <returns></returns>
        System.Int32 GCPerformGC();

        /// <summary>
        /// see how much memory is available
        /// </summary>
        /// <returns></returns>
        System.Int32 GCGetApproxMemory();
#endif

    }
}