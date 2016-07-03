// -----------------------------------------------------------------------
//  <copyright file="DebugOptionsTabHarness.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows.Controls;
using ServiceSentry.Client.UNTESTED.ViewModels;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Controls
{
    /// <summary>
    ///     Interaction logic for DebugOptionsTabHarness.xaml
    /// </summary>
    public abstract partial class DebugOptionsTabHarness
    {
        internal static DebugOptionsTabHarness GetInstance(DebugOptionsViewModel viewModel)
        {
            return new DOTabHarnessImplementation(viewModel);
        }

        private sealed class DOTabHarnessImplementation : DebugOptionsTabHarness
        {
            public DOTabHarnessImplementation(DebugOptionsViewModel viewModel)
            {
                InitializeComponent();
                DebugOptionsTab.DataContext = viewModel;
            }

            public override TabItem GetTabItem()
            {
                var tab = DebugOptionsTab;
                Harness.Items.Remove(tab);
                return tab;
            }
        }

        public abstract TabItem GetTabItem();
    }
}