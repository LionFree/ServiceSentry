// -----------------------------------------------------------------------
//  <copyright file="NotificationOptionsTabHarness.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

#endregion

#region References

using System.Windows.Controls;
using ServiceSentry.Client.UNTESTED.ViewModels;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Controls
{
    /// <summary>
    ///     Interaction logic for NotificationOptionsView.xaml
    /// </summary>
    public abstract partial class NotificationOptionsTabHarness
    {
        internal static NotificationOptionsTabHarness GetInstance(NotificationOptionsViewModel viewModel)
        {
            return new NOTabHarnessImplementation(viewModel);
        }

        public abstract TabItem GetTabItem();

        private sealed class NOTabHarnessImplementation : NotificationOptionsTabHarness
        {
            public NOTabHarnessImplementation(NotificationOptionsViewModel viewModel)
            {
                InitializeComponent();
                NotificationOptionsTab.DataContext = viewModel;
            }

            public override TabItem GetTabItem()
            {
                var tab = NotificationOptionsTab;
                Harness.Items.Remove(tab);
                return tab;
            }
        }
    }
}