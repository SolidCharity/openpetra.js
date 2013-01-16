This code is from a sample provided by Microsoft. See the EULA.doc included in this directory.
The article and source for this code is at: http://msdn.microsoft.com/en-us/magazine/cc163879.aspx

We are using this as an alternative to the Mono XSP server on Windows, as long as we need a custom built mono server for ext.net

see http://stackoverflow.com/questions/4019466/httplistener-access-denied-c-sharp-windows-7
see http://blogs.msdn.com/b/paulwh/archive/2007/05/04/addressaccessdeniedexception-http-could-not-register-url-http-8080.aspx,
and http://blogs.msdn.com/b/drnick/archive/2006/10/16/configuring-http-for-windows-vista.aspx
netsh http show urlacl
netsh http add urlacl url=http://+:8888/ user=CT69734\tpokorra
netsh http del urlacl url=http://+:8888/