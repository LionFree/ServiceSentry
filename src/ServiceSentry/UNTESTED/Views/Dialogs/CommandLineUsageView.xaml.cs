// -----------------------------------------------------------------------
//  <copyright file="CommandLineUsageView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References



#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for CommandLineUsageView.xaml
    /// </summary>
    public abstract partial class CommandLineUsageView
    {
        internal static CommandLineUsageView GetInstance()
        {
            return new CLUsageViewImplementation();
        }

        private sealed class CLUsageViewImplementation : CommandLineUsageView
        {
            internal CLUsageViewImplementation()
            {
                InitializeComponent();
            }
        }
    }
}