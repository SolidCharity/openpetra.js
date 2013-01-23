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
using System.Threading;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Ict.Common;
using Ict.Common.IO;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Server;

namespace Ict.Common.Remoting.Client
{
    /// connect to the server and return a response
    public class THttpConnector
    {
        private static string ServerURL = string.Empty;

        /// <summary>
        /// initialise the name of the server
        /// </summary>
        /// <param name="AServerURL"></param>
        public static void InitConnection(string AServerURL)
        {
            ServerURL = AServerURL;
        }

        private static SortedList <string, string>ConvertParameters(SortedList <string, object>parameters)
        {
            SortedList <string, string>result = new SortedList <string, string>();

            if (parameters == null)
            {
                return result;
            }

            foreach (string param in parameters.Keys)
            {
                object o = parameters[param];
                result.Add(param, THttpBinarySerializer.SerializeObject(o));
            }

            return result;
        }

        private static string TrimResult(string result)
        {
            // returns <string xmlns="...">someresulttext</string>
            if (TLogging.DebugLevel > 0)
            {
                TLogging.Log("returned from server (unmodified): " + result);
            }

            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty).Substring(result.IndexOf("<"));
            result = result.Substring(result.IndexOf(">") + 1);
            result = result.Substring(0, result.IndexOf("<"));

            // TLogging.Log("returned from server: " + result);
            return result;
        }

        /// <summary>
        /// call a webconnector
        /// </summary>
        public static List <object>CallWebConnector(
            string AModuleName,
            string methodname,
            SortedList <string, object>parameters, string expectedReturnType)
        {
            SortedList <string, string>Parameters = ConvertParameters(parameters);

            string result = THTTPUtils.ReadWebsite(ServerURL + "/server" + AModuleName + ".asmx/" + methodname.Replace(".", "_"), Parameters);

            if ((result == null) || (result.Length == 0))
            {
                throw new Exception("invalid response from the server");
            }

            if (expectedReturnType == "void")
            {
                // TODO check if we got a positive response at all
                return null;
            }

            result = TrimResult(result);

            List <object>resultObjects = new List <object>();

            if (expectedReturnType == "list")
            {
                string[] resultlist = result.Split(new char[] { ',' });

                foreach (string o in resultlist)
                {
                    string[] typeAndVal = o.Split(new char[] { ':' });
                    resultObjects.Add(THttpBinarySerializer.DeserializeObject(typeAndVal[0], typeAndVal[1]));
                }
            }
            else
            {
                resultObjects.Add(THttpBinarySerializer.DeserializeObject(result, expectedReturnType));
            }

            return resultObjects;
        }

        /// <summary>
        /// call a method of a UIConnector
        /// </summary>
        public static List <object>CallUIConnectorMethod(
            Guid ObjectID,
            string AModuleName,
            string UIConnectorClass,
            string methodname,
            SortedList <string, object>parameters, string expectedReturnType)
        {
            parameters.Add("UIConnectorObjectID", ObjectID);

            return CallWebConnector(AModuleName, UIConnectorClass + "." + methodname, parameters, expectedReturnType);
        }

        /// <summary>
        /// read a property of a UIConnector
        /// </summary>
        public static object ReadUIConnectorProperty(
            Guid ObjectID,
            string AModuleName,
            string UIConnectorClass,
            string propertyname,
            string expectedReturnType)
        {
            SortedList <string, object>parameters = new SortedList <string, object>();
            parameters.Add("UIConnectorObjectID", ObjectID);

            return CallWebConnector(AModuleName, UIConnectorClass + ".Get" + propertyname, parameters, expectedReturnType)[0];
        }

        /// <summary>
        /// write to a property of a UIConnector
        /// </summary>
        public static void WriteUIConnectorProperty(
            Guid ObjectID,
            string AModuleName,
            string UIConnectorClass,
            string propertyname,
            object value)
        {
            SortedList <string, object>parameters = new SortedList <string, object>();
            parameters.Add("UIConnectorObjectID", ObjectID);
            parameters.Add("value", value);

            CallWebConnector(AModuleName, UIConnectorClass + ".Set" + propertyname, parameters, "void");
        }

        /// <summary>
        /// create a UIConnector on the server
        /// </summary>
        public static Guid CreateUIConnector(
            string AModuleName,
            string classname,
            SortedList <string, object>parameters)
        {
            SortedList <string, string>Parameters =
                ConvertParameters(parameters);

            string result = THTTPUtils.ReadWebsite(ServerURL + "/server" + AModuleName + ".asmx/Create_" + classname, Parameters);

            result = TrimResult(result);

            return Guid.Parse(THttpBinarySerializer.DeserializeObject(result, "System.String").ToString());
        }

        /// <summary>
        /// create a UIConnector on the server, that depends on another UIConnector object
        /// </summary>
        public static Guid GetDependantUIConnector(
            Guid ParentObjectID,
            string AModuleName,
            string UIConnectorClass,
            string propertyname)
        {
            SortedList <string, object>Parameters = new SortedList <string, object>();
            Parameters.Add("UIConnectorObjectID", ParentObjectID);

            string result = CallWebConnector(AModuleName, UIConnectorClass + ".Get" + propertyname, Parameters, "System.String")[0].ToString();

            return Guid.Parse(result);
        }

        /// <summary>
        /// disconnect an object from the server, freeing up memory on the server
        /// </summary>
        public static void Disconnect(Guid ObjectID)
        {
            SortedList <string, object>Parameters = new SortedList <string, object>();
            Parameters.Add("UIConnectorObjectID", ObjectID);

            CallWebConnector("SessionManager", "DisconnectUIConnector", Parameters, "void");
        }
    }
}