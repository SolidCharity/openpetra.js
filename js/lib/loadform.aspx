<%@ Page Language="C#" %>
<%@ Assembly Name="System.IO" %>
<%@ Import Namespace="System.IO" %>
<%
    Response.ContentType = "text/html";
    Response.CacheControl = "no-cache";
    string form = Request.QueryString["form"];

    if (form.Length > 0 && !form.Contains("/"))
    {
		using (StreamReader sr = new StreamReader("forms/" + form + ".html"))
		{
			string content = sr.ReadToEnd();
			// TODO: do we only need the body of the document? or is there a javascript section in the head?
			// better: search for a <form>.js file and include the code in this write
			content = content.Substring(content.IndexOf("<body"));
			content = content.Substring(0, content.IndexOf("</body>") + "</body>".Length);
			Response.Write(content);
		}
    }
%>
