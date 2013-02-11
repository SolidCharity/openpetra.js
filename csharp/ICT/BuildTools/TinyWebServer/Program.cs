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
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Net;
using System.IO;
using System.Reflection;

namespace Ict.Tools.TinyWebServer
{
/// <summary>
/// this is a simple ASMX Server, for use if XSP from mono is not available
/// </summary>
    class TTinyASMXServer
    {
        static void Main(string[] args)
        {
            try
            {
                string physicalDir = Directory.GetCurrentDirectory();

                if (!(physicalDir.EndsWith(Path.DirectorySeparatorChar.ToString())))
                {
                    physicalDir = physicalDir + Path.DirectorySeparatorChar;
                }

                // Copy this hosting DLL into the /bin directory of the application
                string FileName = Assembly.GetExecutingAssembly().Location;

                try
                {
                    if (!Directory.Exists(physicalDir + "bin" + Path.DirectorySeparatorChar))
                    {
                        Directory.CreateDirectory(physicalDir + "bin" + Path.DirectorySeparatorChar);
                    }

                    File.Copy(FileName, physicalDir + "bin" + Path.DirectorySeparatorChar + Path.GetFileName(FileName), true);
                }
                catch
                {
                    ;
                }

                ThreadedHttpListenerWrapper thlw = (ThreadedHttpListenerWrapper)ApplicationHost.CreateApplicationHost(
                    typeof(ThreadedHttpListenerWrapper), "/", physicalDir);

                string port = "8888";
                string[] parameters = Environment.GetCommandLineArgs();

                if (parameters.Length > 1)
                {
                    port = parameters[1];
                }

                Console.WriteLine("trying to listen on port " + port);

                string[] prefixes = new string[] {
                    "http://+:" + port + "/"
                };

                thlw.Configure(prefixes, "/", Directory.GetCurrentDirectory());

                try
                {
                    thlw.Start();
                }
                catch (HttpListenerException)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("we cannot listen on this port. perhaps you need to run as administrator once: ");
                    Console.WriteLine(
                        "  netsh http add urlacl url=http://+:" + port + "/ user=" + Environment.MachineName + "\\" + Environment.UserName);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();

                    throw;
                }

                Console.WriteLine("Listening for requests on http://127.0.0.1:" + port + "/");

                while (true)
                {
                    thlw.ProcessRequest();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}