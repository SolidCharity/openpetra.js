// Auto generated by nant generateGlue
// From a template at inc\template\src\ClientServerGlue\Interface.cs
//
// Do not modify this file manually!
//
{#GPLFILEHEADER}

using System;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using Ict.Common;
using Ict.Common.Data;
using Ict.Common.Verification;
using Ict.Common.Remoting.Shared;
{#USINGNAMESPACES}

namespace Ict.Petra.Shared.Interfaces.M{#MODULE}
{
    {#INTERFACES}
}

{##INTERFACE}
/// <summary>auto generated</summary>
public interface {#INTERFACENAME} : IInterface
{
    {#CONTENT}
}

{##CONNECTORPROPERTY}
/// <summary>auto generated from Connector property ({#SERVERNAMESPACE}.{#CONNECTORCLASSNAME})</summary>
{#TYPE} {#NAME}
{
{#IFDEF GETTER}
    get;
{#ENDIF GETTER}
{#IFDEF SETTER}
    set;
{#ENDIF SETTER}
}