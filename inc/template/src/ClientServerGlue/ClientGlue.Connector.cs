// Auto generated by nant generateGlue
// From a template at inc\template\src\ClientServerGlue\ClientGlue.Connector.cs
//
// Do not modify this file manually!
//
{#GPLFILEHEADER}

using System;
using System.IO;
using System.Threading;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Ict.Common;
using Ict.Common.Remoting.Shared;
{#USINGNAMESPACES}

namespace Ict.Common.Remoting.Client
{
    /// generated code because the standalone application will link to the server,
    /// but the client application should not contain all the server dlls
    public class TConnectionHelper
    {
        /// connect to the server
        static public IClientManager Connect()
        {
            {#CONNECTOR}
        }
    }

    {#STANDALONECLIENTMANAGER}
}

{##CONNECTORSTANDALONE}
// this is code for the standalone openpetra, there is no server, only one single application
Thread.CurrentThread.Name = "StandaloneOpenPetra";
DomainManager.GClientID = 0;

TServerManager TheServerManager = new TServerManager();

//
// Connect to main Database
//
try
{
    TheServerManager.EstablishDBConnection();
}
catch (FileNotFoundException ex)
{
    TLogging.Log(ex.Message);
    TLogging.Log("Please check your OpenPetra.build.config file ...");
    TLogging.Log("Maybe a nant initConfigFile helps ...");
    throw new ApplicationException();
}
catch (Exception)
{
    throw;
}

StringHelper.CurrencyFormatTable = DBAccess.GDBAccessObj.SelectDT("SELECT * FROM PUB_a_currency", "a_currency", null);

return new TStandaloneClientManager();

{##CONNECTORCLIENTSERVER}
return new THTTPClientManager();

{##STANDALONECLIENTMANAGER}
/// <summary>
/// client manager for standalone
/// </summary>
public class TStandaloneClientManager : IClientManager
{
    /// check the user name and password
    public void ConnectClient(String AUserName,
        String APassword,
        String AClientComputerName,
        String AClientIPAddress,
        System.Version AClientExeVersion,
        TClientServerConnectionType AClientServerConnectionType,
        out String AClientName,
        out System.Int32 AClientID,
        out string ACrossDomainURL,
        out TExecutingOSEnum AServerOS,
        out Int32 AProcessID,
        out String AWelcomeMessage,
        out Boolean ASystemEnabled,
        out IPrincipal AUserInfo)
    {
        // TODORemoting
        AClientName = string.Empty;
        AClientID = -1;
        ACrossDomainURL = string.Empty;
        AServerOS = TExecutingOSEnum.eosLinux;
        AProcessID = -1;
        AWelcomeMessage = string.Empty;
        ASystemEnabled = true;

        try
        {
            TClientManager.ConnectClient(
                AUserName,
                APassword,
                "localhost",
                "127.0.0.1",
                new Version(TClientInfo.ClientAssemblyVersion),
                TClientServerConnectionType.csctLocal,
                out AClientID,
                out AWelcomeMessage,
                out ASystemEnabled,
                out AUserInfo);
        }
        catch (Exception e)
        {
            TLogging.Log(e.Message);
            TLogging.Log(e.StackTrace);

            // Login not successful
            throw new EAccessDeniedException();
        }

        AUserInfo = UserInfo.GUserInfo;
    }

    /// <summary>
    /// disconnect
    /// </summary>
    public Boolean DisconnectClient(System.Int32 AClientID, out String ACantDisconnectReason)
    {
        ACantDisconnectReason = string.Empty;
        return true;
    }

    /// <summary>
    /// disconnect
    /// </summary>
    public Boolean DisconnectClient(System.Int32 AClientID, String AReason, out String ACantDisconnectReason)
    {
        ACantDisconnectReason = string.Empty;
        return true;
    }
}