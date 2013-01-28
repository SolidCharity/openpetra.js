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
using System.Net.Security;
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

                // see http://blogs.msdn.com/b/carloc/archive/2007/02/13/webclient-2-0-class-not-working-under-win2000-with-https.aspx
                // it seems we need to specify SSL3 instead of TLS
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

                // see http://stackoverflow.com/questions/566437/http-post-returns-the-error-417-expectation-failed-c
                System.Net.ServicePointManager.Expect100Continue = false;

                if (TAppSettingsManager.GetValue("IgnoreServerCertificateValidation", "false", false) == "true")
                {
                    // when checking the validity of a SSL certificate, always pass
                    // this only makes sense in a testing environment, with self signed certificates
                    ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(
                            delegate
                            { return true; }
                            );
                }
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

        private static string WebClientUploadValues(string url, NameValueCollection parameters, int ANumberOfAttempts = 0)
        {
            byte[] buf;

            if (FWebClient == null)
            {
                FWebClient = new WebClientWithSession();
            }

            try
            {
                buf = FWebClient.UploadValues(url, parameters);
            }
            catch (System.NotSupportedException)
            {
                // System.NotSupportedException: WebClient does not support concurrent I/O operations
                FWebClient = new WebClientWithSession(FWebClient.CookieContainer);
                buf = FWebClient.UploadValues(url, parameters);
            }
            catch (System.Net.WebException)
            {
                if (ANumberOfAttempts > 0)
                {
                    // sleep for half a second
                    System.Threading.Thread.Sleep(500);
                    return WebClientUploadValues(url, parameters, ANumberOfAttempts - 1);
                }

                throw;
            }

            if ((buf != null) && (buf.Length > 0))
            {
                return Encoding.ASCII.GetString(buf, 0, buf.Length);
            }

            return String.Empty;
        }

        /// <summary>
        /// post a request to a website. used for Connectors
        /// </summary>
        public static string PostRequest(string url, NameValueCollection parameters)
        {
            if (TLogging.DebugLevel > 0)
            {
                LogRequest(url, parameters);
            }

            try
            {
                return WebClientUploadValues(url, parameters, 10);
            }
            catch (System.Net.WebException e)
            {
                TLogging.Log("Trying to download: ");
                LogRequest(url, parameters);
                TLogging.Log(e.Message);
            }

            return String.Empty;
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
            if (FWebClient == null)
            {
                FWebClient = new WebClientWithSession();
            }

            try
            {
                FWebClient.DownloadFile(url, filename);
                return true;
            }
            catch (Exception e)
            {
                TLogging.Log(e.Message + " url: " + url + " filename: " + filename);
            }

            return false;
        }
    }
}