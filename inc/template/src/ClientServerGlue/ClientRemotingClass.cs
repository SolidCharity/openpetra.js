// Auto generated by nant generateGlue
// From a template at inc\template\src\ClientServerGlue\RemotableObjects.cs
//
// Do not modify this file manually!
//
{#GPLFILEHEADER}

using System;
using Ict.Common.Remoting.Client;

namespace Ict.Petra.Server.M{#TOPLEVELMODULE}.Instantiator
{
    {#CLASSES}
}

{##CLASS}
/// object that will be serialized to the client.
/// it opens a new channel for each new object. 
/// this is needed for cross domain marshalling.
[Serializable]
public class {#CLASSNAME}: {#INTERFACE}
{
    private {#INTERFACE} RemoteObject = null;
    private string FObjectURI;
    /// constructor
    public {#CLASSNAME}(string AObjectURI)
    { 
        FObjectURI = AObjectURI;
    }
    private void InitRemoteObject()
    { 
        RemoteObject = ({#INTERFACE})
            TConnector.TheConnector.GetRemoteObject(FObjectURI,
            typeof({#INTERFACE}));
    }
    {#METHODSANDPROPERTIES}
}

{##PROPERTY}
/// forward the property
public {#TYPE} {#NAME}
{
{#IFDEF GETTER}
    get
    {
        if (RemoteObject == null) { InitRemoteObject(); }
        return RemoteObject.{#NAME};
    }
{#ENDIF GETTER}
{#IFDEF SETTER}
    get
    {
        if (RemoteObject == null) { InitRemoteObject(); }
        RemoteObject.{#NAME} = value;
    }
{#ENDIF SETTER}
}

{##METHOD}
/// forward the method call
public {#RETURNTYPE} {#METHODNAME}({#PARAMETERDEFINITION})
{
    if (RemoteObject == null) { InitRemoteObject(); }
    {#RETURN}RemoteObject.{#METHODNAME}({#ACTUALPARAMETERS});
}