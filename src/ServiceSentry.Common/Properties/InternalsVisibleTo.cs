// -----------------------------------------------------------------------
//  <copyright file="InternalsVisibleTo.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Runtime.CompilerServices;

#endregion

[assembly: InternalsVisibleTo("ServiceSentry.Model.UnitTests")]
[assembly: InternalsVisibleTo("ServiceSentry.UnitTests")]
[assembly: InternalsVisibleTo("ServiceSentry.ServiceFramework.UnitTests")]
[assembly: InternalsVisibleTo("ServiceSentry.Monitor.UnitTests")]
[assembly: InternalsVisibleTo("ServiceSentry.AVS.UnitTests")]
[assembly: InternalsVisibleTo("ServiceSentry.Common.UnitTests")]
[assembly: InternalsVisibleTo("ServiceSentry.Common.Testing")]

// This assembly is the default dynamic assembly generated Castle DynamicProxy, 
// used by Moq. Paste in a single line. 
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]