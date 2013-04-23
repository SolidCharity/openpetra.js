EPPlus is a .net library that reads and writes Excel 2007/2010 files using the Open Office Xml format (xlsx).

http://epplus.codeplex.com/

licensed under LGPL

current version:
modified version based on Mercurial from 11 April 2013 (version 3.1.3)

my own branch:
http://epplus.codeplex.com/SourceControl/network/forks/tpokorra/monoWorkarounds
hg clone https://hg.codeplex.com/forks/tpokorra/monoworkarounds

merge from tip:
hg pull https://hg.codeplex.com/epplus
hg merge
resolve conflicts manually, then run: hg resolve --mark
vi ~/.hgrc
 [ui]
 username = Timotheus Pokorra <timotheus.pokorra@solidcharity.com>
hg commit
hg push --branch default --new-branch

Problems with Mono:

see http://epplus.codeplex.com/workitem/13096   
and https://bugzilla.xamarin.com/show_bug.cgi?id=2527

https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/System.IO.Packaging/PackUriHelper.cs
https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/System.IO.Packaging/PackagePart.cs

For testing xlsx files without owning Microsoft Office, and for more detailed error messages:
http://www.microsoft.com/en-us/download/details.aspx?id=5124
Open XML SDK 2.0 for Microsoft Office
C:\Program Files (x86)\Open XML SDK\V2.0\tool