// -----------------------------------------------------------------------
//  <copyright file="MonitorServiceInstaller.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ComponentModel;
using System.Configuration.Install;
using ServiceSentry.Common.ServiceFramework;

#endregion

namespace ServiceSentry.Monitor.Infrastructure
{
    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class MonitorServiceInstaller : Installer
    {
        // creates a blank windows service installer with configuration in MonitorWindowsService
        public MonitorServiceInstaller()
        {
            var installers = WindowsServiceInstaller.GetInstance<MonitorService>();
            Installers.Add(installers.ServiceInstaller);
            Installers.Add(installers.ProcessInstaller);
        }
    }
}