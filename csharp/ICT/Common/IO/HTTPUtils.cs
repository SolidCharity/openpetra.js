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
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Ict.Common.IO
{
    /// <summary>
    /// a few simple functions to access content from the web
    /// </summary>
    public class THTTPUtils
    {
        private class WebClientWithSession : WebClient
        {
            public WebClientWithSession()
                : this(new CookieContainer())
            {
            }

            public WebClientWithSession(CookieContainer c)
            {
                this.CookieContainer = c;
            }

            public CookieContainer CookieContainer {
                get; set;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);

                var castRequest = request as HttpWebRequest;

                if (castRequest != null)
                {
                    castRequest.CookieContainer = this.CookieContainer;
                }

                return request;
            }
        }

        private static WebClientWithSession FWebClient = null;

        /// <summary>
        /// read from a website;
        /// used to check for available patches
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ReadWebsite(string url)
        {
            string ReturnValue = null;

            // see http://blogs.msdn.com/b/carloc/archive/2007/02/13/webclient-2-0-class-not-working-under-win2000-with-https.aspx
            // it seems we need to specify SSL3 instead of TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

            byte[] buf;

            if (FWebClient == null)
            {
                FWebClient = new WebClientWithSession();
            }

            if (TLogging.DebugLevel > 0)
            {
                string urlToLog = url;

                if (url.Contains("password"))
                {
                    urlToLog = url.Substring(0, url.IndexOf("?")) + "?...";
                }

                TLogging.Log(urlToLog);
            }

            try
            {
                buf = FWebClient.DownloadData(url);

                if ((buf != null) && (buf.Length > 0))
                {
                    ReturnValue = Encoding.ASCII.GetString(buf, 0, buf.Length);
                }
                else
                {
                    TLogging.Log("server did not return anything? timeout?");
                }
            }
            catch (System.Net.WebException e)
            {
                if (url.Contains("?"))
                {
                    // do not show passwords in the log file which could be encoded in the parameters
                    TLogging.Log("Trying to download: " + url.Substring(0, url.IndexOf("?")) + "?..." + Environment.NewLine +
                        e.Message, TLoggingType.ToLogfile);
                }
                else
                {
                    TLogging.Log("Trying to download: " + url + Environment.NewLine +
                        e.Message, TLoggingType.ToLogfile);
                }
            }

            return ReturnValue;
        }

        private static void LogRequest(string url, NameValueCollection parameters)
        {
            TLogging.Log(url);

            foreach (string k in parameters.Keys)
            {
                if (k.ToLower().Contains("password"))
                {
                    TLogging.Log(" " + k + " = *****");
                }
                else
                {
                    TLogging.Log(" " + k + " = " + parameters[k]);
                }
            }
        }

        /// <summary>
        /// post a request to a website. used for Connectors
        /// </summary>
        public static string PostRequest(string url, NameValueCollection parameters)
        {
            string ReturnValue = null;

            // see http://blogs.msdn.com/b/carloc/archive/2007/02/13/webclient-2-0-class-not-working-under-win2000-with-https.aspx
            // it seems we need to specify SSL3 instead of TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

            byte[] buf;

            if (FWebClient == null)
            {
                FWebClient = new WebClientWithSession();
            }

            if (TLogging.DebugLevel > 0)
            {
                LogRequest(url, parameters);
            }

            try
            {
                buf = FWebClient.UploadValues(url, parameters);

                if ((buf != null) && (buf.Length > 0))
                {
                    ReturnValue = Encoding.ASCII.GetString(buf, 0, buf.Length);
                }
                else
                {
                    TLogging.Log("server did not return anything? timeout?");
                }
            }
            catch (System.Net.WebException e)
            {
                TLogging.Log("Trying to download: ");
                LogRequest(url, parameters);
                TLogging.Log(e.Message);
            }

            return ReturnValue;
        }

        /// <summary>
        /// download a patch or other file from a website;
        /// used for patching the program
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Boolean DownloadFile(string url, string filename)
        {
            // see http://blogs.msdn.com/b/carloc/archive/2007/02/13/webclient-2-0-class-not-working-under-win2000-with-https.aspx
            // it seems we need to specify SSL3 instead of TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

            Boolean ReturnValue = false;
            WebClient client = new WebClient();

            try
            {
                client.DownloadFile(url, filename);
                ReturnValue = true;
            }
            catch (Exception e)
            {
                TLogging.Log(e.Message + " url: " + url + " filename: " + filename);
            }
            return ReturnValue;
        }
    }
}