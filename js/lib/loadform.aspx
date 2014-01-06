<%@ Page Language="C#" %>
<%@ Assembly Name="System.IO" %>
<%@ Assembly Name="Ict.Common" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Ict.Common" %>
<%
    Response.ContentType = "text/html";
    Response.CacheControl = "no-cache";
    string form = Request.QueryString["form"];

    if (form.Length > 0 && !form.Contains("/"))
    {
        string content;
        string path = TAppSettingsManager.GetValue("Forms.Path");

        using (StreamReader sr = new StreamReader(path + "/" + form + ".html"))
        {
            content = sr.ReadToEnd();

            // we only need the body of the document
            if (content.IndexOf("<body") != -1)
            {
                content = content.Substring(content.IndexOf("<body"));
                content = content.Substring(0, content.IndexOf("</body>") + "</body>".Length);
            }
        }

        // search for a <form>.js file and include the code in this write
        string javascript = string.Empty;
        if (File.Exists(path + "/" + form + ".js"))
        {
            using (StreamReader sr = new StreamReader(path + "/" + form + ".js"))
            {
                javascript = "<script>" + sr.ReadToEnd() + "</script>";
            }
        }
        
        Response.Write(javascript + content);
    }
%>
