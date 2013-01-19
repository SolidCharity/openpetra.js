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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.ServiceModel.Web;
using System.ServiceModel;
using Ict.Common;
using Ict.Common.Remoting.Server;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Client;
using Tests.HTTPRemoting.Interface;

namespace Tests.HTTPRemoting.Service
{
    /// <summary>
    /// the test service
    /// </summary>
    [WebService(Namespace = "http://www.openpetra.org/webservices/Sample")]
    [ScriptService]
    public class TMyService : WebService
    {
        /// <summary>
        /// constructor, which is called for each http request
        /// </summary>
        public TMyService() : base()
        {
            TOpenPetraOrgSessionManagerTest.Init();
        }

        /// <summary>
        /// print hello world
        /// </summary>
        /// <param name="msg"></param>
        [WebMethod(EnableSession = true)]
        public string HelloWorld(string msg)
        {
            TLogging.Log(msg);
            return "Hello from the server!!!";
        }

        /// <summary>
        /// some tests for remoting DateTime objects
        /// </summary>
        /// <param name="date"></param>
        /// <param name="outDate"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public DateTime TestDateTime(DateTime date, out DateTime outDate)
        {
            Console.WriteLine("ToShortDateString(): " + date.ToShortDateString());
            Console.WriteLine("ToUniversalTime(): " + date.ToUniversalTime());
            Console.WriteLine("ToLocalTime(): " + date.ToLocalTime());

            date = new DateTime(date.Year, date.Month, date.Day);
            Console.WriteLine("ToShortDateString(): " + date.ToShortDateString());
            Console.WriteLine("ToUniversalTime(): " + date.ToUniversalTime());
            Console.WriteLine("ToLocalTime(): " + date.ToLocalTime());

            outDate = date;
            return date;
        }
    }

    /// <summary>
    /// server manager for testing purposes
    /// </summary>
    public class TOpenPetraOrgSessionManagerTest
    {
        private static string TheServerManager = null;

        /// <summary>
        /// initialise the server once for everyone
        /// </summary>
        public static bool Init()
        {
            if (TheServerManager == null)
            {
                // make sure the correct config file is used
                new TAppSettingsManager(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "web.config");
                new TSrvSetting();
                new TLogging(TSrvSetting.ServerLogFile);
                TLogging.DebugLevel = TAppSettingsManager.GetInt16("Server.DebugLevel", 0);

                Catalog.Init();

                TheServerManager = "test";

                TLogging.Log("Server has been initialised");

                return true;
            }

            return false;
        }
    }
}