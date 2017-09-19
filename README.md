System.DirectoryServices Sample
===============================

System.DirectoryServices Background
-----------------------------------

Work to add System.DirectoryServices.* to .NET Core is [in progress](https://github.com/dotnet/corefx/issues/2089).

Current expectation is that the following libraries should be available for
.NET Core around the time of .NET Core 2.0 RTM (or shortly thereafter):

* System.DirectoryServices
* System.DirectoryServices.AccountManagement
* System.DirectoryServices.ActiveDirectory
* System.DirectoryServices.Protocols

However, the libraries will initially only work on Windows (as they have a
number of Win32 dependencies). Once initial versions are available for
Windows, cross-plat porting work will begin, but
System.DirectoryServices.Protocols is the only library expected to port
easily cross-platform (as it is a lower-level LDAP library whereas the
others are AD-specific).

This Sample's Purpose
---------------------

Because a number of customers have asked about APIs in
System.DirectoryServices (especially DirectoryEntry, DirectorySearcher,
and related search and property types) for use in simple querying scenarios,
this sample demonstrates how much of the graph-reading capabilities of the
System.DirectoryServices library can be achieved with
System.DirectoryServices.Protocols, instead.

The small ad-hoc test app calls some of the DirectoryServices APIs used most
often by customers. The System.DirectoryServices.Protocols.Services project
exposes those APIs and depends only on System.DirectoryServices.Protocols
(not System.DirectoryServices) so the ad-hoc test app can switch back and
forth between System.DirectoryServices and the
System.DirectoryServices.Protocols.Services shim by just changing a 'using'
statement.

At the moment, this solution targets Net452, but once
System.DirectoryServices.Protocols has stabilized it should be easy to update
it to target NetCoreApp2.0 instead.

Disclaimer
----------

Although this sample demonstrates how to mimic some useful
System.DirectoryServices APIs in a manner that will work cross-platform
sooner than System.DirectoryServices will, it is far from a complete
System.DirectoryServices replacement. For example, this sample currently only
supports reading data from AD (data cannot be changed or added). Hopefully it
can serve as a model, though, for customers looking to transition from
System.DirectoryServices to System.DirectoryServices.Protocols as a way to
prepare for cross-platform AD/LDAP scenarios.

Resources
---------

Useful further reading for folks interested in DirectoryServices topics:

* [System.DirectoryServices.Protocols documentation](https://msdn.microsoft.com/en-us/library/bb332056.aspx)
* [System.DirectoryServices documentation](https://msdn.microsoft.com/en-us/library/system.directoryservices%28v=vs.110%29.aspx)
* [Work item tracking DirectoryServices implementation for .NET Core](https://github.com/dotnet/corefx/issues/2089)
* This is specifically about querying information from an AD graph (useful for authorization). For authentication, users may want to investigate [ADAL](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-libraries)
  * [ADAL v3 supports .NET Core](https://blogs.technet.microsoft.com/enterprisemobility/2016/05/18/adal-net-v3-reaches-ga/)
