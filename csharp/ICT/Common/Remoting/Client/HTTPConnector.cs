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

        /// <summary>
        /// serialize any object. if it is a complex type, use Base64
        /// </summary>
        static public string SerializeObject(object o, bool binary)
        {
            if (!binary)
            {
                return o.ToString();
            }

            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, o);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        /// <summary>
        /// reverse of SerializeObject
        /// </summary>
        static public object DeserializeObject(string s, string type)
        {
            if (type == "System.Int64")
            {
                return Convert.ToInt64(s);
            }
            else if (type == "System.Int32")
            {
                return Convert.ToInt32(s);
            }
            else if (type == "System.Int16")
            {
                return Convert.ToInt16(s);
            }
            else if (type == "System.String")
            {
                return s;
            }
            else if (type == "binary")
            {
                MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(s));
                memoryStream.Seek(0, SeekOrigin.Begin);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(memoryStream);
            }
            else
            {
                TLogging.Log("HttpConnector.DeserializeObject: unexpeced type: " + type);
                return null;
            }
        }

        private static SortedList <string, string>ConvertParameters(SortedList <string, object>parameters)
        {
            SortedList <string, string>result = new SortedList <string, string>();

            foreach (string param in parameters.Keys)
            {
                object o = parameters[param];
                result.Add(param, SerializeObject(o,
                        !(o.GetType() == typeof(string)
                          || o.GetType() == typeof(Int16)
                          || o.GetType() == typeof(Int32)
                          || o.GetType() == typeof(Int64)
                          )));
            }

            return result;
        }

        private static string TrimResult(string result)
        {
            // returns <string xmlns="...">someresulttext</string>
            TLogging.Log("returned from server (unmodified): " + result);
            result = result.Substring(result.IndexOf("<string xmlns="));
            result = result.Substring(result.IndexOf(">") + 1);
            result = result.Substring(0, result.IndexOf("<"));

            TLogging.Log("returned from server: " + result);
            return result;
        }

        /// <summary>
        /// call a webconnector
        /// </summary>
        public static List <object>CallWebConnector(
            string methodname,
            SortedList <string, object>parameters, string expectedReturnType)
        {
            SortedList <string, string>Parameters = ConvertParameters(parameters);

            string result = THTTPUtils.ReadWebsite(ServerURL + "/" + methodname, Parameters);

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
                    resultObjects.Add(DeserializeObject(typeAndVal[0], typeAndVal[1]));
                }
            }
            else
            {
                resultObjects.Add(DeserializeObject(result, expectedReturnType));
            }

            return resultObjects;
        }

        /// <summary>
        /// call a method of a UIConnector
        /// </summary>
        public static List <object>CallUIConnectorMethod(
            Guid ObjectID,
            string UIConnectorClass,
            string methodname,
            SortedList <string, object>parameters, string expectedReturnType)
        {
            parameters.Add("UIConnectorObjectID", ObjectID);

            return CallWebConnector(UIConnectorClass + "." + methodname, parameters, expectedReturnType);
        }

        /// <summary>
        /// read a property of a UIConnector
        /// </summary>
        public static object ReadUIConnectorProperty(
            Guid ObjectID,
            string UIConnectorClass,
            string propertyname,
            string expectedReturnType)
        {
            SortedList <string, object>parameters = new SortedList <string, object>();
            parameters.Add("UIConnectorObjectID", ObjectID);

            return CallWebConnector(UIConnectorClass + ".Get" + propertyname, parameters, expectedReturnType)[0];
        }

        /// <summary>
        /// write to a property of a UIConnector
        /// </summary>
        public static void WriteUIConnectorProperty(
            Guid ObjectID,
            string UIConnectorClass,
            string propertyname,
            object value)
        {
            SortedList <string, object>parameters = new SortedList <string, object>();
            parameters.Add("UIConnectorObjectID", ObjectID);
            parameters.Add("value", value);

            CallWebConnector(UIConnectorClass + ".Set" + propertyname, parameters, "void");
        }

        /// <summary>
        /// create a UIConnector on the server
        /// </summary>
        public static Guid CreateUIConnector(
            string classname,
            SortedList <string, object>parameters)
        {
            SortedList <string, string>Parameters =
                ConvertParameters(parameters);

            string result = THTTPUtils.ReadWebsite(ServerURL + "/" + classname, Parameters);

            result = TrimResult(result);

            return Guid.Parse(DeserializeObject(result, "System.String").ToString());
        }

        /// <summary>
        /// create a UIConnector on the server, that depends on another UIConnector object
        /// </summary>
        public static Guid GetDependantUIConnector(
            Guid ParentObjectID,
            string UIConnectorClass,
            string propertyname)
        {
            SortedList <string, object>Parameters = new SortedList <string, object>();
            Parameters.Add("UIConnectorObjectID", ParentObjectID);

            string result = CallWebConnector(UIConnectorClass + ".Get" + propertyname, Parameters, "System.String")[0].ToString();

            return Guid.Parse(result);
        }

        /// <summary>
        /// disconnect an object from the server, freeing up memory on the server
        /// </summary>
        public static void Disconnect(Guid ObjectID)
        {
            SortedList <string, object>Parameters = new SortedList <string, object>();
            Parameters.Add("UIConnectorObjectID", ObjectID);

            CallWebConnector("DisconnectUIConnector", Parameters, "void");
        }
    }
}