// -----------------------------------------------------------------------
//  <copyright file="InternalsVisibleTo.cs" company="Curtis Kaler">
//      Copyright (c) 2013 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Runtime.CompilerServices;

#endregion

[assembly: InternalsVisibleTo("ServiceSentry.Model")]

[assembly: InternalsVisibleTo("ServiceSentry.UnitTests")]
[assembly: InternalsVisibleTo("ServiceSentry.AVS.UnitTests")]

// This assembly is the default dynamic assembly generated Castle DynamicProxy, 
// used by Moq. Paste in a single line. 
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]