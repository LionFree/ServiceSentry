// -----------------------------------------------------------------------
//  <copyright file="AutoStart.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Extensibility;

//using ServiceSentry.Resources;

#endregion

namespace ServiceSentry.Client.UNTESTED.Infrastructure
{
    /// <summary>
    ///     Taken from: http://simpcode.blogspot.com/2008/07/c-set-and-unset-auto-start-for-windows.html
    /// </summary>
    internal abstract class AutoStart
    {
        /// <summary>
        ///     The long version of the command line argument which
        ///     signals the application that it should start minimized.
        /// </summary>
        public const string MinimizedArgumentLong = "minimized";

        /// <summary>
        ///     The short version of the command line argument which
        ///     signals the application that it should start minimized.
        /// </summary>
        public const string MinimizedArgumentShort = "m";

        /// <summary>
        ///     Checks whether the auto-start option is enabled.
        /// </summary>
        internal abstract bool IsAutoStartEnabled { get; }

        /// <summary>
        ///     Path of the executable to run when Windows starts.
        /// </summary>
        internal abstract string StartupPath { get; }

        /// <summary>
        ///     Enables / disables auto start.
        /// </summary>
        /// <param name="isEnabled">Set true to enable auto start.</param>
        internal abstract void SetAutoStart(bool isEnabled);

        internal static AutoStart GetInstance()
        {
            return GetInstance(WindowsRegistry.Default);
        }

        internal static AutoStart GetInstance(WindowsRegistry registry)
        {
            return new AutoStartImplementation(registry);
        }

        private sealed class AutoStartImplementation : AutoStart
        {
            /// <summary>
            ///     The registery path that hosts automatically started applications
            ///     for the current user.
            /// </summary>
            private const string AutoStartKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

            /// <summary>
            ///     Application registry key.
            /// </summary>
            private readonly string _applicationWindowsRegistryKey = Extensibility.Strings._CompanyName +
                                                              " " +
                                                              Extensibility.Strings._ApplicationName;

            private readonly WindowsRegistry _registry;

            /// <summary>
            ///     The string in the autostart key ("&lt;path&gt;\ServiceSentry.exe" -minimized)
            /// </summary>
            private readonly string _startupPath;

            internal AutoStartImplementation(WindowsRegistry registry)
            {
                _registry = registry;
                _startupPath = string.Format("\"{0}\" -{1}",
                                             AssemblyWrapper.Default.GetEntryAssembly().Location,
                                             MinimizedArgumentLong);
            }

            internal override string StartupPath
            {
                get { return _startupPath; }
            }

            internal override bool IsAutoStartEnabled
            {
                get
                {
                    var key = _registry.CurrentUser.ReadSubKey(AutoStartKey);
                    if (key == null) return false;

                    var value = (string) key.GetValue(_applicationWindowsRegistryKey);
                    if (value == null) return false;

                    return (value == _startupPath);
                }
            }

            internal override void SetAutoStart(bool isEnabled)
            {
                if (isEnabled)
                {
                    var key = _registry.CurrentUser.CreateSubKey(AutoStartKey);
                    if (key != null) key.SetValue(_applicationWindowsRegistryKey, _startupPath);
                }
                else
                {
                    var key = _registry.CurrentUser.CreateSubKey(AutoStartKey);
                    if (key != null) key.DeleteValue(_applicationWindowsRegistryKey, false);
                }
            }
        }
    }
}