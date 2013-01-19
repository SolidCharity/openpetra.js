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
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using Ict.Common;
using Ict.Common.Data; // Implicit reference
using Ict.Common.DB;
using Ict.Common.IO;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Server;
using Ict.Petra.Server.App.Core;
using Ict.Petra.Server.App.Core.Security;
using Ict.Petra.Shared.Interfaces.MFinance;
using Ict.Petra.Shared.Security;
using Ict.Petra.Server.MFinance.AP.UIConnectors;
using Ict.Petra.Server.MConference.Applications;
using Ict.Common.Verification;
using Ict.Petra.Shared.MFinance.AP.Data;
using Jayrock.Json;
using Ict.Petra.Server.MPartner.Import;
using Ict.Petra.Shared;

namespace OpenPetraWebService
{
/// <summary>
/// this publishes the SOAP web services of OpenPetra.org
/// </summary>
[WebService(Namespace = "http://www.openpetra.org/webservices/SessionManager")]
[ScriptService]
public class TOpenPetraOrgSessionManager : WebService
{
    /// <summary>
    /// static: only initialised once for the whole server
    /// </summary>
    static TServerManager TheServerManager = null;

    /// <summary>
    /// constructor, which is called for each http request
    /// </summary>
    public TOpenPetraOrgSessionManager() : base()
    {
        TOpenPetraOrgSessionManager.Init();
    }

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

            DBAccess.SetFunctionForRetrievingCurrentObjectFromWebSession(SetDatabaseForSession,
                GetDatabaseFromSession);

            UserInfo.SetFunctionForRetrievingCurrentObjectFromWebSession(SetUserInfoForSession,
                GetUserInfoFromSession);

            Catalog.Init();

            TheServerManager = new TServerManager();
            try
            {
                TheServerManager.EstablishDBConnection();

                TSystemDefaultsCache.GSystemDefaultsCache = new TSystemDefaultsCache();
                DomainManager.GSiteKey = TSystemDefaultsCache.GSystemDefaultsCache.GetInt64Default(
                    Ict.Petra.Shared.SharedConstants.SYSDEFAULT_SITEKEY);
            }
            catch (Exception e)
            {
                TLogging.Log(e.Message);
                TLogging.Log(e.StackTrace);
                throw;
            }

            TLogging.Log("Server has been initialised");

            return true;
        }

        return false;
    }

    private bool LoginInternal(string username, string password)
    {
        Int32 ProcessID;
        bool ASystemEnabled;

        try
        {
            TClientManager.PerformLoginChecks(
                username.ToUpper(), password.Trim(), "WEB", "127.0.0.1", out ProcessID, out ASystemEnabled);
            Session["LoggedIn"] = true;

            DBAccess.GDBAccessObj.UserID = username.ToUpper();

            TheServerManager.AddDBConnection(DBAccess.GDBAccessObj);

            return true;
        }
        catch (Exception e)
        {
            TLogging.Log(e.Message);
            TLogging.Log(e.StackTrace);
            Session["LoggedIn"] = false;
            Ict.Common.DB.DBAccess.GDBAccessObj.RollbackTransaction();
            DBAccess.GDBAccessObj.CloseDBConnection();
            return false;
        }
    }

    /// <summary>Login a user</summary>
    [WebMethod(EnableSession = true)]
    public bool Login(string username, string password)
    {
        bool loggedIn = LoginInternal(username, password);

        return loggedIn;
    }

    static private TDataBase GetDatabaseFromSession()
    {
        if (HttpContext.Current.Session["DBAccessObj"] == null)
        {
            if (TheServerManager == null)
            {
                TLogging.Log("GetDatabaseFromSession : TheServerManager is null");
            }
            else
            {
                // disconnect web user after 2 minutes of inactivity. should disconnect itself already earlier
                TheServerManager.DisconnectTimedoutDatabaseConnections(2 * 60, "ANONYMOUS");

                // disconnect normal users after 3 hours of inactivity
                TheServerManager.DisconnectTimedoutDatabaseConnections(3 * 60 * 60, "");

                TheServerManager.EstablishDBConnection();
            }
        }

        return (TDataBase)HttpContext.Current.Session["DBAccessObj"];
    }

    static private void SetDatabaseForSession(TDataBase database)
    {
        HttpContext.Current.Session["DBAccessObj"] = database;
    }

    static private TPetraPrincipal GetUserInfoFromSession()
    {
        return (TPetraPrincipal)HttpContext.Current.Session["UserInfo"];
    }

    static private void SetUserInfoForSession(TPetraPrincipal userinfo)
    {
        HttpContext.Current.Session["UserInfo"] = userinfo;
    }

    /// <summary>check if the user has logged in successfully</summary>
    [WebMethod(EnableSession = true)]
    public bool IsUserLoggedIn()
    {
        object loggedIn = Session["LoggedIn"];

        if ((null != loggedIn) && ((bool)loggedIn == true))
        {
            return true;
        }

        return false;
    }

    /// <summary>log the user out</summary>
    [WebMethod(EnableSession = true)]
    public bool Logout()
    {
        TLogging.Log("Logout from a session", TLoggingType.ToLogfile | TLoggingType.ToConsole);

        if (DBAccess.GDBAccessObj != null)
        {
            DBAccess.GDBAccessObj.CloseDBConnection();
        }

        // Session Abandon causes problems in Mono 2.10.x see https://bugzilla.novell.com/show_bug.cgi?id=669807
        // TODO Session.Abandon();
        Session.Clear();

        return true;
    }
}
}