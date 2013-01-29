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
using System.Collections.Generic;
using System.Security.Principal;
using Ict.Common;
using Ict.Common.IO;
using Ict.Common.Remoting.Shared;

namespace Ict.Common.Remoting.Client
{
    /// client manager for the connection to the server via http
    public class TClientManager
    {
        /// connect the client to the server
        public static void ConnectClient(String AUserName,
            String APassword,
            String AClientComputerName,
            String AClientIPAddress,
            System.Version AClientExeVersion,
            TClientServerConnectionType AClientServerConnectionType,
            out String AClientName,
            out System.Int32 AClientID,
            out string ACrossDomainURL,
            out TExecutingOSEnum AServerOS,
            out Int32 AProcessID,
            out String AWelcomeMessage,
            out Boolean ASystemEnabled,
            out IPrincipal AUserInfo)
        {
            // TODORemoting
            AClientName = string.Empty;
            AClientID = -1;
            ACrossDomainURL = string.Empty;
            AServerOS = TExecutingOSEnum.eosLinux;
            AProcessID = -1;
            AWelcomeMessage = string.Empty;
            ASystemEnabled = true;

            THttpConnector.InitConnection(TAppSettingsManager.GetValue("OpenPetra.HTTPServer"));
            SortedList <string, object>Parameters = new SortedList <string, object>();
            Parameters.Add("username", AUserName);
            Parameters.Add("password", APassword);

            if ((bool)THttpConnector.CallWebConnector("SessionManager", "Login", Parameters, "System.Boolean")[0] == false)
            {
                // failed login
                // TODORemoting
                throw new EAccessDeniedException();
            }

            AUserInfo = (IPrincipal)THttpConnector.CallWebConnector("SessionManager", "GetUserInfo", null, "binary")[0];
        }

        /// <summary>
        /// disconnect
        /// </summary>
        public static Boolean DisconnectClient(System.Int32 AClientID, out String ACantDisconnectReason)
        {
            // TODORemoting
            ACantDisconnectReason = string.Empty;
            return true;
        }

        /// <summary>
        /// disconnect
        /// </summary>
        public static Boolean DisconnectClient(System.Int32 AClientID, String AReason, out String ACantDisconnectReason)
        {
            // TODORemoting
            ACantDisconnectReason = string.Empty;
            return true;
        }

        /**
         * Can be called to queue a ClientTask for a certain Client.
         *
         * See implementation of this class for more detailed description!
         *
         */
        public static Int32 QueueClientTaskFromClient(System.Int32 AClientID,
            String ATaskGroup,
            String ATaskCode,
            object ATaskParameter1,
            object ATaskParameter2,
            object ATaskParameter3,
            object ATaskParameter4,
            System.Int16 ATaskPriority,
            System.Int32 AExceptClientID)
        {
            // TODORemoting
            return -1;
        }

        /**
         * Can be called to queue a ClientTask for a certain Client.
         *
         * See implementation of this class for more detailed description!
         *
         */
        public static Int32 QueueClientTaskFromClient(String AUserID,
            String ATaskGroup,
            String ATaskCode,
            object ATaskParameter1,
            object ATaskParameter2,
            object ATaskParameter3,
            object ATaskParameter4,
            System.Int16 ATaskPriority,
            System.Int32 AExceptClientID)
        {
            // TODORemoting
            return -1;
        }

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
        public static void AddErrorLogEntry(String AErrorCode,
            String AContext,
            String AMessageLine1,
            String AMessageLine2,
            String AMessageLine3,
            String AUserID,
            Int32 AProcessID)
        {
            // TODORemoting
        }

        /**
         * The following functions are only for development purposes (note that these
         * functions can also be invoked directly from the Server's menu)
         *
         */
        public static System.Int32 GCGetGCGeneration(object AInspectObject)
        {
            // TODORemoting
            return -1;
        }

        /// <summary>
        /// perform garbage collection
        /// </summary>
        /// <returns></returns>
        public static System.Int32 GCPerformGC()
        {
            // TODORemoting
            return -1;
        }

        /// <summary>
        /// see how much memory is available
        /// </summary>
        /// <returns></returns>
        public static System.Int32 GCGetApproxMemory()
        {
            // TODORemoting
            return -1;
        }
    }
}