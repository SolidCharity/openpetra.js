<%@ Page Language="C#" %>
<%@ Assembly Name="System.IO" %>
<%@ Import Namespace="System.IO" %>
<%
    Response.ContentType = "text/html";
    Response.CacheControl = "no-cache";
    string form = Request.QueryString["form"];

    if (form.Length > 0 && !form.Contains("/"))
    {
        string content;
        
        using (StreamReader sr = new StreamReader("forms/" + form + ".html"))
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
        if (File.Exists("forms/" + form + ".js"))
        {
            using (StreamReader sr = new StreamReader("forms/" + form + ".js"))
            {
                javascript = "<script>" + sr.ReadToEnd() + "</script>";
            }
        }
        
        Response.Write(javascript + content);
    }
%>
