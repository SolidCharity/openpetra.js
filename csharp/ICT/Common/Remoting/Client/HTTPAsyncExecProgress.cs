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
        private string FObjectID = string.Empty;
        private string FModule = string.Empty;

        /// constructor, this UIConnector is created from the property of another UIConnector
        public TAsynchronousExecutionProgress(string AModule, string ObjectID)
        {
            FObjectID = ObjectID;
            FModule = AModule;
        }

        /// desctructor
        ~TAsynchronousExecutionProgress()
        {
            Dispose();
        }

        /// dispose the object on the server as well
        public void Dispose()
        {
            THttpConnector.DisconnectUIConnector(FModule, FObjectID);
        }

        /// <summary>
        /// some text information about current progress
        /// </summary>
        public String ProgressInformation
        {
            get
            {
                return (string)THttpConnector.ReadUIConnectorProperty(FObjectID,
                    FModule,
                    "TAsynchronousExecutionProgress",
                    "ProgressInformation",
                    "System.String");
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
                    FModule,
                    "TAsynchronousExecutionProgress",
                    "ProgressPercentage",
                    "System.Int16");
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
                    FModule,
                    "TAsynchronousExecutionProgress",
                    "ProgressState",
                    "binary");
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public object Result
        {
            get
            {
                return THttpConnector.ReadUIConnectorProperty(FObjectID, FModule, "TAsynchronousExecutionProgress", "Result", "binary");
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
                FModule,
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
            THttpConnector.CallUIConnectorMethod(FObjectID,
                FModule,
                "TAsynchronousExecutionProgress",
                "Cancel",
                ActualParameters,
                "void");
        }
    }
}