// -----------------------------------------------------------------------
//  <copyright file="WindowsServiceManager.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Globalization;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using ServiceSentry.Common.Resources;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Common.ServiceFramework
{
    // Manages the windows service.
    internal abstract class WindowsServiceManager
    {
        #region Factory Methods

        public static WindowsServiceManager GetInstance(ServiceMetadata metadata,
            ConsoleHarness harness,
            bool logToConsole)
        {
            return GetInstance(metadata,
                               harness,
                               AssemblyWrapper.Default,
                               ManagedInstaller.Default,
                               ControllerWrapper.GetInstance(metadata.ServiceName),
                               logToConsole);
        }

        internal static WindowsServiceManager GetInstance(
            ServiceMetadata metadata,
            ConsoleHarness harness,
            AssemblyWrapper assemblyWrapper,
            ManagedInstaller installer,
            ControllerWrapper controller,
            bool logToConsole)
        {
            return new Implementation(metadata, harness, assemblyWrapper, installer, controller, logToConsole);
        }

        #endregion

        #region Abstract Members

        public abstract ServiceControllerStatus? Status { get; }

        /// <summary>
        ///     Installs the <see cref="WindowsService" />.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool Install();

        /// <summary>
        ///     Installs and attempts to start the <see cref="WindowsService" />.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool InstallAndStart();

        /// <summary>
        ///     Displays the <see cref="ServiceControllerStatus" /> of the <see cref="WindowsService" /> in the <see cref="Console" />.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool ShowStatus();

        /// <summary>
        ///     Attempts to start the <see cref="WindowsService" />, passing no arguments.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool StartService();

        /// <summary>
        ///     Attempts to start the <see cref="WindowsService" />, passing the designated arguments.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool StartService(string[] args);

        /// <summary>
        ///     Attempts to stop the <see cref="WindowsService" />.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool StopService();

        /// <summary>
        ///     Uninstalls the <see cref="WindowsService" />.
        /// </summary>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool Uninstall();

        #endregion

        private sealed class Implementation : WindowsServiceManager
        {
            private readonly AssemblyWrapper _assemblyWrapper;
            private readonly ControllerWrapper _controller;
            private readonly ConsoleHarness _harness;
            private readonly ManagedInstaller _installer;
            private readonly bool _logToConsole;
            private readonly bool _quiet;
            private readonly string _serviceName;
            private readonly bool _silent;
            
            public Implementation(ServiceMetadata metadata,
                                  ConsoleHarness harness,
                                  AssemblyWrapper assemblyWrapper,
                                  ManagedInstaller installer,
                                  ControllerWrapper controller,
                                  bool logToConsole)
            {
                _assemblyWrapper = assemblyWrapper;
                _controller = controller;
                _harness = harness;
                _installer = installer;
                _logToConsole = logToConsole;
                _quiet = metadata.Quiet;
                _serviceName = metadata.ServiceName;
                _silent = metadata.Silent;
            }

            public override ServiceControllerStatus? Status
            {
                get
                {
                    try
                    {
                        return _controller.Status;
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                    return null;
                }
            }

            public override bool Install()
            {
                return InstallService(true);
            }

            public override bool Uninstall()
            {
                var assemblyLocation = _assemblyWrapper.GetEntryAssembly().Location;

                WriteLine();
                Write("Uninstalling '{0}' service...  ", _serviceName);
                try
                {
                    _installer.InstallHelper(new[]
                        {
                            "/u",
                            "/LogToConsole=" + _logToConsole,
                            assemblyLocation
                        });

                    WriteLine("Done.");
                    return true;
                }
                catch (InstallException ex)
                {
                    return HandleException(ex);
                }
                catch (InvalidOperationException ex)
                {
                    return HandleException(ex);
                }
            }

            public override bool InstallAndStart()
            {
                var success = InstallService(true);
                if (!success) return false;

                success = StartService();
                if (!success) return false;

                return true;
            }

            public override bool StartService(string[] args)
            {
                try
                {
                    if (_controller.Status != ServiceControllerStatus.Stopped)
                    {
                        WriteError();
                        WriteError("The {0} service is already running.", _serviceName);
                        return false;
                    }

                    WriteLine();
                    Write("Starting {0} service...  ", _serviceName);

                    if (args == null) _controller.Start();
                    else _controller.Start(args);

                    _controller.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 0, 30));
                    WriteLine("Done.");
                    return true;
                }
                catch (Exception ex)
                {
                    return HandleException(ex);
                }
            }

            public override bool StartService()
            {
                return StartService(null);
            }

            public override bool StopService()
            {
                try
                {
                    if (_controller.Status == ServiceControllerStatus.Stopped)
                    {
                        WriteError();
                        WriteError("The {0} service is not started.", _serviceName);
                        return false;
                    }

                    WriteLine();
                    Write("Stopping {0} service...  ", _serviceName);

                    if (_controller.CanStop)
                    {
                        _controller.Stop();
                        _controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 0, 30));
                        WriteLine("Done.");

                        return true;
                    }

                    WriteError();
                    WriteError("Cannot stop service.");
                    return false;
                }
                catch (Exception ex)
                {
                    return HandleException(ex);
                }
            }

            public override bool ShowStatus()
            {
                try
                {
                    var status = Status;
                    if (status == null) return false;

                    _harness.WriteLine();
                    _harness.WriteLine("{0} service is {1}.", _serviceName, status);
                    return true;
                }
                catch (Exception ex)
                {
                    return HandleException(ex);
                }
            }

            private bool InstallService(bool firstTry)
            {
                WriteLine();
                Write(CommonStrings.Info_InstallingWindowsService, _serviceName);
                
                try
                {
                    var assemblyLocation = _assemblyWrapper.GetEntryAssembly().Location;
                    _installer.InstallHelper(new[]
                        {
                            "/LogToConsole=" + _logToConsole,
                            assemblyLocation
                        });

                    WriteLine(CommonStrings.Info_Done);
                    return true;
                }
                catch (InvalidOperationException ex)
                {
                    if (firstTry && IsAlreadyInstalled(ex))
                    {
                        OverWriteIfNecessary();
                        return true;
                    }

                    return HandleException(ex);
                }
            }

            private void OverWriteIfNecessary()
            {
                const int sameVersion = 0;
                const int newerVersion = 1;
                var existingVersion = InstalledVersionComparison();

                if (existingVersion == sameVersion)
                {
                    WriteLine(CommonStrings.Info_Done);
                    return;
                }
                if (existingVersion == newerVersion)
                {
                    if (!WantToOverwriteService())
                    {
                        return;
                    }
                }
                
                // Uninstall this version.
                Uninstall();

                // Try to install.
                InstallService(false);
            }

            private bool WantToOverwriteService()
            {
                // Check if we want to overwrite.
                _harness.WriteLine(ConsoleColor.Yellow, CommonStrings.Warn_InstalledVersionIsNewer);
                _harness.WriteLine();
                _harness.Write(ConsoleColor.Yellow, CommonStrings.Warn_InstallAnyway, CommonStrings.Noun_YesKey, CommonStrings.Noun_NoKey);
                var key = _harness.ReadKey();
                _harness.WriteLine();

                return key.KeyChar.ToString(CultureInfo.CurrentUICulture) == CommonStrings.Noun_YesKey;
            }

            private int InstalledVersionComparison()
            {
                var servicePath = GetExistingServicePath();
                var installedVersion = _assemblyWrapper.Version(servicePath);
                var thisVersion = _assemblyWrapper.Version();

                return installedVersion.CompareTo(thisVersion);
            }

            private string GetExistingServicePath()
            {
                var serviceKeyString = @"SYSTEM\CurrentControlSet\Services\" + _serviceName;

                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(serviceKeyString);
                if (key == null) throw new Exception(CommonStrings.EXCEPTION_CannotFindServiceInRegistry);

                var path = key.GetValue("ImagePath").ToString();
                key.Close();


                if (path.StartsWith("\""))
                {
                    path = Regex.Match(path, "\"([^\"]+)\"").Groups[1].Value;
                }

                return Environment.ExpandEnvironmentVariables(path);
            }


            private static bool IsAlreadyInstalled(Exception ex)
            {
                if (ex.InnerException != null)
                {
                    var inEx = ex.InnerException as Win32Exception;
                    if (inEx != null)
                    {
                        return (inEx.ErrorCode == -2147467259);
                    }
                }
                return false;
            }


            private void Write(string format, params object[] args)
            {
                if (!(_quiet || _silent))
                {
                    _harness.Write(format, args);
                }
            }

            private void WriteLine(string format, params object[] args)
            {
                if (!(_quiet || _silent))
                {
                    _harness.WriteLine(format, args);
                }
            }

            private void WriteLine()
            {
                if (!(_quiet || _silent))
                {
                    _harness.WriteLine();
                }
            }

            private void WriteError(string format, params object[] args)
            {
                if (!_silent)
                {
                    _harness.WriteLine(format, args);
                }
            }

            private void WriteError()
            {
                if (!_silent)
                {
                    _harness.WriteLine();
                }
            }

            private bool HandleException(Exception ex)
            {
                if (ex is InstallException)
                {
                    if (!_silent)
                    {
                        _harness.WriteLine();
                        _harness.WriteToConsole(ConsoleColor.Yellow,
                                                "  The " + _serviceName + " service is not installed.");
                    }
                    return true;
                }

                if (ex is InvalidOperationException)
                {
                    if (ex.GetHashCode() == 55530882)
                    {
                        if (!_silent)
                        {
                            _harness.WriteLine();
                            _harness.WriteToConsole(ConsoleColor.Yellow,
                                                    "  The " + _serviceName + " service is already installed.");
                        }
                        return true;
                    }
                }

                if (ex.InnerException != null)
                {
                    if (ex.InnerException is Win32Exception)
                    {
                        if (!_silent)
                        {
                            _harness.WriteLine();
                            _harness.WriteToConsole(ConsoleColor.Yellow,
                                                    "  The " + _serviceName + " service is already running.");
                        }
                        return true;
                    }

                    _harness.WriteLine();
                    _harness.WriteToConsole(ConsoleColor.Yellow, ex.InnerException.GetType().FullName);
                    _harness.WriteToConsole(ConsoleColor.Yellow, ex.InnerException.Message);
                    _harness.WriteToConsole(ConsoleColor.Yellow,
                                            ex.InnerException.GetHashCode().ToString(CultureInfo.InvariantCulture));
                }


                return false;
            }
        }
    }

    internal abstract class ControllerWrapper
    {
        public abstract ServiceControllerStatus Status { get; }
        public abstract bool CanStop { get; }

        public static ControllerWrapper GetInstance(string serviceName)
        {
            return new Implementation(serviceName);
        }

        public abstract void Start();
        public abstract void Start(string[] args);
        public abstract void Stop();
        public abstract void WaitForStatus(ServiceControllerStatus status, TimeSpan timeout);
        public abstract void WaitForStatus(ServiceControllerStatus status);

        private sealed class Implementation : ControllerWrapper
        {
            private readonly string _serviceName;

            public Implementation(string serviceName)
            {
                _serviceName = serviceName;
            }

            public override ServiceControllerStatus Status
            {
                get
                {
                    using (var sc = new ServiceController(_serviceName))
                    {
                        return sc.Status;
                    }
                }
            }

            public override bool CanStop
            {
                get
                {
                    using (var sc = new ServiceController(_serviceName))
                    {
                        return sc.CanStop;
                    }
                }
            }

            public override void Start()
            {
                using (var sc = new ServiceController(_serviceName))
                {
                    sc.Start();
                }
            }

            public override void Start(string[] args)
            {
                using (var sc = new ServiceController(_serviceName))
                {
                    sc.Start(args);
                }
            }

            public override void Stop()
            {
                using (var sc = new ServiceController(_serviceName))
                {
                    sc.Stop();
                }
            }

            public override void WaitForStatus(ServiceControllerStatus status, TimeSpan timeout)
            {
                using (var sc = new ServiceController(_serviceName))
                {
                    sc.WaitForStatus(status, timeout);
                }
            }

            public override void WaitForStatus(ServiceControllerStatus status)
            {
                WaitForStatus(status, TimeSpan.MaxValue);
            }
        }
    }
}