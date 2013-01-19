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

namespace Ict.Common.Remoting.Shared
{
    /// serialize and deserialize complex types in binary
    public class THttpBinarySerializer
    {
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

        /// serialize any object. depending on the type of the object, it will serialized in binary format
        static public string SerializeObject(object o)
        {
            return SerializeObject(o, !(o.GetType() == typeof(string)
                                        || o.GetType() == typeof(Int16)
                                        || o.GetType() == typeof(Int32)
                                        || o.GetType() == typeof(Int64)
                                        || o.GetType() == typeof(bool)
                                        ));
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
            else if (type == "System.Boolean")
            {
                return Convert.ToBoolean(s);
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
    }
}