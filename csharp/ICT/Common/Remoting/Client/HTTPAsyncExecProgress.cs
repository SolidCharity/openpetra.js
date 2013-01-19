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
using Ict.Common;
using Ict.Common.IO;
using Ict.Common.Remoting.Shared;

namespace Ict.Common.Remoting.Client
{
    /// manually written client side for this special UIConnector
    public class TAsynchronousExecutionProgress : IAsynchronousExecutionProgress, IDisposable
    {
        private Guid FObjectID = new Guid();

        /// constructor, this UIConnector is created from the property of another UIConnector
        public TAsynchronousExecutionProgress(Guid ObjectID)
        {
            FObjectID = ObjectID;
        }

        /// desctructor
        ~TAsynchronousExecutionProgress()
        {
            Dispose();
        }

        /// dispose the object on the server as well
        public void Dispose()
        {
            THttpConnector.Disconnect(FObjectID);
        }

        /// <summary>
        /// some text information about current progress
        /// </summary>
        public String ProgressInformation
        {
            get
            {
                return (string)THttpConnector.ReadUIConnectorProperty(FObjectID,
                    "SessionManager",
                    "TAsynchronousExecutionProgress",
                    "ProgressInformation",
                    "System.String");
            }
            set
            {
                THttpConnector.WriteUIConnectorProperty(FObjectID, "SessionManager", "TAsynchronousExecutionProgress", "ProgressInformation", value);
            }
        }

        /// <summary>
        /// progress percentage
        /// </summary>
        public Int16 ProgressPercentage
        {
            get
            {
                return (Int16)THttpConnector.ReadUIConnectorProperty(FObjectID,
                    "SessionManager",
                    "TAsynchronousExecutionProgress",
                    "ProgressPercentage",
                    "System.Int16");
            }
            set
            {
                THttpConnector.WriteUIConnectorProperty(FObjectID, "SessionManager", "TAsynchronousExecutionProgress", "ProgressPercentage", value);
            }
        }

        /// <summary>
        /// progress state
        /// </summary>
        public TAsyncExecProgressState ProgressState
        {
            get
            {
                return (TAsyncExecProgressState)THttpConnector.ReadUIConnectorProperty(FObjectID,
                    "SessionManager",
                    "TAsynchronousExecutionProgress",
                    "ProgressState",
                    "binary");
            }
            set
            {
                THttpConnector.WriteUIConnectorProperty(FObjectID, "SessionManager", "TAsynchronousExecutionProgress", "ProgressState", value);
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public object Result
        {
            get
            {
                return THttpConnector.ReadUIConnectorProperty(FObjectID, "SessionManager", "TAsynchronousExecutionProgress", "Result", "binary");
            }
            set
            {
                THttpConnector.WriteUIConnectorProperty(FObjectID, "SessionManager", "TAsynchronousExecutionProgress", "Result", value);
            }
        }

        /// <summary>
        /// get all 3 properties at once
        /// </summary>
        /// <param name="ProgressState"></param>
        /// <param name="ProgressPercentage"></param>
        /// <param name="ProgressInformation"></param>
        public void ProgressCombinedInfo(out TAsyncExecProgressState ProgressState, out Int16 ProgressPercentage, out String ProgressInformation)
        {
            SortedList <string, object>ActualParameters = new SortedList <string, object>();
            List <object>Result = THttpConnector.CallUIConnectorMethod(FObjectID,
                "SessionManager",
                "TAsynchronousExecutionProgress",
                "ProgressCombinedInfo",
                ActualParameters,
                "list");
            ProgressState = (TAsyncExecProgressState)Result[0];
            ProgressPercentage = (System.Int16)Result[1];
            ProgressInformation = (System.String)Result[2];
        }

        /// <summary>
        /// cancel the operation that is monitored
        /// </summary>
        public void Cancel()
        {
            SortedList <string, object>ActualParameters = new SortedList <string, object>();
            List <object>Result = THttpConnector.CallUIConnectorMethod(FObjectID,
                "SessionManager",
                "TAsynchronousExecutionProgress",
                "Cancel",
                ActualParameters,
                "void");
        }
    }
}