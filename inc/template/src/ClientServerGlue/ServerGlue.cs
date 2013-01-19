// Auto generated by nant generateGlue
// From a template at inc\template\src\ClientServerGlue\ServerGlue.cs
//
// Do not modify this file manually!
//
{#GPLFILEHEADER}

using System;
using System.Threading;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.ServiceModel.Web;
using System.ServiceModel;
using Ict.Common;
using Ict.Common.Data;
using Ict.Common.Verification;
using Ict.Common.Remoting.Shared;
using Ict.Petra.Shared;
{#USINGNAMESPACES}

namespace OpenPetraWebService
{
/// <summary>
/// this publishes the SOAP web services of OpenPetra.org for module {#TOPLEVELMODULE}
/// </summary>
[WebService(Namespace = "http://www.openpetra.org/webservices/M{#TOPLEVELMODULE}")]
[ScriptService]
public class TM{#TOPLEVELMODULE}WebService : WebService
{
    /// <summary>
    /// constructor, which is called for each http request
    /// </summary>
    public TM{#TOPLEVELMODULE}WebService() : base()
    {
        TOpenPetraOrgSessionManager.Init();
    }

    {#WEBCONNECTORS}
}
}

{##WEBCONNECTOR}
/// web connector method call
public {#RETURNTYPE} {#WEBCONNECTORCLASS}_{#METHODNAME}({#PARAMETERDEFINITION})
{
    {#CHECKUSERMODULEPERMISSIONS}
    {#LOCALVARIABLES}
    {#LOCALRETURN}{#WEBCONNECTORCLASS}.{#METHODNAME}({#ACTUALPARAMETERS});
    {#RETURN}
}

{##CHECKUSERMODULEPERMISSIONS}
TModuleAccessManager.CheckUserPermissionsForMethod(typeof({#CONNECTORWITHNAMESPACE}), "{#METHODNAME}", "{#PARAMETERTYPES}"{#LEDGERNUMBER});
