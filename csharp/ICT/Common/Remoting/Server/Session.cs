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
using System.Web;
using Ict.Common;

namespace Ict.Common.Remoting.Server
{
    /// <summary>
    /// Static class for storing sessions.
    /// we are using our own session handling,
    /// since the .net sessions cannot handle concurrent requests in one session if there is a write block
    /// </summary>
    public class TSession
    {
        private static SortedList <string, SortedList <string, object>>FSessionObjects = new SortedList <string, SortedList <string, object>>();

        private static string GetSessionID()
        {
            if (HttpContext.Current.Request.Cookies["OpenPetraSessionID"] != null)
            {
                return HttpContext.Current.Request.Cookies["OpenPetraSessionID"].Value;
            }

            return string.Empty;
        }

        private static SortedList <string, object>GetSession()
        {
            string sessionID = GetSessionID();

            if ((sessionID != string.Empty) && !FSessionObjects.ContainsKey(sessionID))
            {
                HttpContext.Current.Request.Cookies.Remove("OpenPetraSessionID");
                sessionID = GetSessionID();
            }

            if (sessionID == string.Empty)
            {
                sessionID = Guid.NewGuid().ToString();
                HttpContext.Current.Request.Cookies.Add(new HttpCookie("OpenPetraSessionID", sessionID));
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("OpenPetraSessionID", sessionID));
                FSessionObjects.Add(sessionID, new SortedList <string, object>());
            }

            return FSessionObjects[sessionID];
        }

        /// <summary>
        /// set a session variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetVariable(string name, object value)
        {
            // HttpContext.Current.Session[name] = value;
            SortedList <string, object>session = GetSession();

            if (session.Keys.Contains(name))
            {
                session[name] = value;
            }
            else
            {
                session.Add(name, value);
            }
        }

        /// <summary>
        /// returns true if variable exists and is not null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool HasVariable(string name)
        {
            SortedList <string, object>session = GetSession();

            if (session.Keys.Contains(name) && (GetSession()[name] != null))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// get a session variable
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object GetVariable(string name)
        {
            // return HttpContext.Current.Session[name];
            SortedList <string, object>session = GetSession();

            if (session.Keys.Contains(name))
            {
                return GetSession()[name];
            }

            return null;
        }

        /// <summary>
        /// clear the current session
        /// </summary>
        static public void Clear()
        {
            // HttpContext.Current.Session.Clear();
            string sessionId = GetSessionID();

            if (sessionId.Length > 0)
            {
                FSessionObjects.Remove(sessionId);
                HttpContext.Current.Request.Cookies.Remove("OpenPetraSessionID");
            }
        }
    }
}