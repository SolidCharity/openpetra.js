<%@ Page Language="C#" %>
<%@ Assembly Name="System.IO" %>
<%@ Assembly Name="Ict.Common" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="Ict.Common" %>
<%
    Response.ContentType = "text/html";
    Response.CacheControl = "no-cache";
    string form = Request.QueryString["form"];

    if (form.Length > 0 && !form.Contains("/"))
    {
        string content = string.Empty;
        string path = TAppSettingsManager.GetValue("Forms.Path");
        StringBuilder result = new StringBuilder();

        // search for a <form>.js file and include the code in the result
        if (File.Exists(path + "/" + form + ".js"))
        {
            using (StreamReader sr = new StreamReader(path + "/" + form + ".js"))
            {
                result.Append("<script>").Append(sr.ReadToEnd()).Append("</script>");
            }
        }

        using (StreamReader sr = new StreamReader(path + "/" + form + ".html"))
        {
            content = sr.ReadToEnd();

            // we only need the body of the document
            if (content.IndexOf("<body") != -1)
            {
                content = content.Substring(content.IndexOf("<body"));
                content = content.Substring(0, content.IndexOf("</body>") + "</body>".Length);
            }

            Int32 previousPosInclude = -1;
            Int32 newPosInclude;
            while ((newPosInclude = content.IndexOf("<!-- include ", previousPosInclude + 1)) != -1)
            {
                if (previousPosInclude != -1)
                {
                    result.Append(content.Substring(previousPosInclude, newPosInclude - previousPosInclude));
                }
                else
                {
                    result.Append(content.Substring(0, newPosInclude));
                }

                string includeFilename = content.Substring(
                    newPosInclude + "<!-- include ".Length,
                    content.IndexOf("-->", newPosInclude) - "<!-- include ".Length - newPosInclude).Trim();

                if (includeFilename.StartsWith("css/") && File.Exists(path + "/../" + includeFilename))
                {
                    using (StreamReader srCss = new StreamReader(path + "/../" + includeFilename))
                    {
                        result.Append("<style>").Append(srCss.ReadToEnd()).Append("</style>");
                    }
                }
                if (includeFilename.StartsWith("js/") && File.Exists(path + "/../" + includeFilename))
                {
                    using (StreamReader srJs = new StreamReader(path + "/../" + includeFilename))
                    {
                        result.Append("<script>").Append(srJs.ReadToEnd()).Append("</script>");
                    }
                }

                previousPosInclude = newPosInclude;
            }

            if (previousPosInclude != -1)
            {
                result.Append(content.Substring(content.IndexOf("-->", previousPosInclude) + "-->".Length));
            }
            else
            {
                result.Append(content);
            }
        }

        Response.Write(result.ToString());
    }
%>
