<%@ Page Language="C#" %>
<%@ Assembly Name="Ict.Petra.Server.app.JsClient" %>
<%@ Import Namespace="Ict.Petra.Server.app.JSClient" %>
<%
    Response.ContentType = "text/html";
    Response.CacheControl = "no-cache";
    string page = Request.QueryString["page"];

    if (page.Length > 0)
    {
        string content = TUINavigation.LoadNavigationPage(page);

        Response.Write(content);
    }
%>
