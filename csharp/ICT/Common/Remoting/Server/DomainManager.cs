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
using System.Web;
using System.Collections.Generic;
using System.Threading;
using Ict.Common;

namespace Ict.Common.Remoting.Server
{
    /// <summary>
    /// holds some global variables, depending on the current session or thread
    /// </summary>
    public class DomainManager
    {
        /// <summary>used internally to hold SiteKey Information (for convenience)</summary>
        public static Int64 GSiteKey;

        private static SortedList <string, Int32>ClientIDsPerThread = new SortedList <string, int>();
        private static SortedList <string, TConnectedClient>CurrentClientPerThread = new SortedList <string, TConnectedClient>();

        /// <summary>
        /// get the ClientID of the current session
        /// </summary>
        public static Int32 GClientID
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    if (!ClientIDsPerThread.ContainsKey(Thread.CurrentThread.Name))
                    {
                        string message = "DomainManager.GClientID is not set in thread " + Thread.CurrentThread.Name;
                        TLogging.Log(message);
                        return 0;
                    }

                    return ClientIDsPerThread[Thread.CurrentThread.Name];
                }
                else
                {
                    return Convert.ToInt32(HttpContext.Current.Session["ClientID"]);
                }
            }

            set
            {
                if (HttpContext.Current == null)
                {
                    ClientIDsPerThread.Add(Thread.CurrentThread.Name, value);
                }
                else
                {
                    HttpContext.Current.Session["ClientID"] = value;
                }
            }
        }

        /// <summary>
        /// the current client in this session
        /// </summary>
        public static TConnectedClient CurrentClient
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    if (!CurrentClientPerThread.ContainsKey(Thread.CurrentThread.Name))
                    {
                        string message = "DomainManager.CurrentClient is not set in thread " + Thread.CurrentThread.Name;
                        TLogging.Log(message);
                        return null;
                    }

                    return CurrentClientPerThread[Thread.CurrentThread.Name];
                }
                else
                {
                    return (TConnectedClient)HttpContext.Current.Session["ConnectedClient"];
                }
            }

            set
            {
                if (HttpContext.Current == null)
                {
                    CurrentClientPerThread.Add(Thread.CurrentThread.Name, value);
                }
                else
                {
                    HttpContext.Current.Session["ConnectedClient"] = value;
                }
            }
        }
    }
}