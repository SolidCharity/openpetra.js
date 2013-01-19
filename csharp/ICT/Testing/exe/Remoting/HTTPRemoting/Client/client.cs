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
using System.Net;
using Ict.Common;
using Ict.Common.Remoting.Client;
using Ict.Common.Remoting.Shared;
using Tests.HTTPRemoting.Interface;
using Tests.HTTPRemoting.Client;

namespace Ict.Testing.HTTPRemoting.Client
{
    class Client
    {
        static void Main(string[] args)
        {
            new TLogging("../../log/TestRemotingClient.log");

            try
            {
                new TAppSettingsManager("../../etc/Client.config");

                TLogging.DebugLevel = Convert.ToInt32(TAppSettingsManager.GetValue("Client.DebugLevel", "0"));

                new TClientSettings();

                // initialize the client
                new TRemote();

                // allow self signed ssl certificate for test purposes
                ServicePointManager.ServerCertificateValidationCallback = delegate {
                    return true;
                };

                THttpConnector.InitConnection(TAppSettingsManager.GetValue("OpenPetra.HTTPServer"));

                TClientInfo.InitializeUnit();

                Catalog.Init("en-GB", "en-GB");

//                IMyUIConnector MyUIConnector = TRemote.MyService.SubNamespace.MyUIConnector();
//                IMySubNamespace test = TRemote.MyService.SubNamespace;

                while (true)
                {
                    try
                    {
                        TLogging.Log("before call");
                        TLogging.Log(TRemote.MyService.HelloWorld("Hello World"));
                        TLogging.Log("after call");
                    }
                    catch (Exception e)
                    {
                        TLogging.Log("problem with MyService HelloWorld: " + Environment.NewLine + e.ToString());
                    }

                    try
                    {
//                        TLogging.Log(test.HelloSubWorld("Hello SubWorld"));
                    }
                    catch (Exception e)
                    {
                        TLogging.Log("problem with sub namespace HelloSubWorld: " + Environment.NewLine + e.ToString());
                    }

                    try
                    {
//                        TLogging.Log(MyUIConnector.HelloWorldUIConnector());
                    }
                    catch (Exception e)
                    {
                        TLogging.Log("problem with HelloWorldUIConnector: " + Environment.NewLine + e.ToString());
                    }

                    Console.WriteLine("Press ENTER to say Hello World again... ");
                    Console.WriteLine("Press CTRL-C to exit ...");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                TLogging.Log(e.ToString());
                Console.ReadLine();
            }
        }
    }
}