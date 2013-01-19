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
using System.Collections.Generic;
using Ict.Common;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Client;
using Tests.HTTPRemoting.Interface;

namespace Tests.HTTPRemoting.Client
{
    /// <summary>
    /// Holds all references to instantiated Serverside objects that are remoted by the Server.
    /// These objects can get remoted either statically (the same Remoting URL all
    /// the time) or dynamically (on-the-fly generation of Remoting URL).
    ///
    /// The TRemote class is used by the Client for all communication with the Server
    /// after the initial communication at Client start-up is done.
    ///
    /// The properties MPartner, MFinance, etc. represent the top-most level of the
    /// Petra Partner, Finance, etc. Petra Module Namespaces in the PetraServer.
    /// </summary>
    public class TRemote
    {
        private class TMyService : IMyService
        {
            /// print hello world
            public string HelloWorld(string msg)
            {
                SortedList <string, object>parameters = new SortedList <string, object>();
                parameters.Add("msg", "hello " + Environment.UserName);
                return (string)THttpConnector.CallWebConnector("Sample", "HelloWorld", parameters, "System.String")[0];
            }

            /// some tests for remoting DateTime objects
            public DateTime TestDateTime(DateTime date, out DateTime outDate)
            {
                // TODORemoting
                outDate = DateTime.Now;
                return DateTime.Now;
            }

            /// get a subnamespace
            public IMySubNamespace SubNamespace
            {
                get
                {
                    // TODORemoting
                    return null;
                }
            }
        }


        /// <summary>Reference to the topmost level of the Petra Common Module Namespace</summary>
        public static IMyService MyService
        {
            get
            {
                return UMyServiceObject;
            }
        }

        private static TMyService UMyServiceObject;

        /// <summary>
        ///
        /// </summary>
        public TRemote()
        {
            UMyServiceObject = new TMyService();
        }
    }
}