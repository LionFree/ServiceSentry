// -----------------------------------------------------------------------
//  <copyright file="IsDesignMode.cs" company="Curtis Kaler">
//      Copyright (c) 2013 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ComponentModel;
using System.Windows;

#endregion

namespace WPFNotifyIcon
{
    public partial class Utilities
    {
        private static readonly bool IsDesignModeBacker;

        static Utilities()
        {
            IsDesignModeBacker =
                (bool)
                DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                                                          typeof (FrameworkElement))
                                            .Metadata.DefaultValue;
        }

        /// <summary>
        ///     Checks whether the application is currently in design mode.
        /// </summary>
        public static bool IsDesignMode
        {
            get { return IsDesignModeBacker; }
        }

        /// <summary>
        ///     Checks whether the application was built in the Debug configuration.
        /// </summary>
        public static bool IsDebugMode
        {
            get { return false; }
        }
    }
}