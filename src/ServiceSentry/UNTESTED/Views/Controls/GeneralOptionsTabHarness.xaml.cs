// -----------------------------------------------------------------------
//  <copyright file="GeneralOptionsTabHarness.xaml.cs" company="Curtis Kaler">
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
    ///     Interaction logic for GeneralOptionsTab.xaml
    /// </summary>
    public abstract partial class GeneralOptionsTabHarness
    {
        internal static GeneralOptionsTabHarness GetInstance(GeneralOptionsViewModel viewModel)
        {
            return new GOTabHarnessImplementation(viewModel);
        }
        public abstract TabItem GetTabItem();

        private sealed class GOTabHarnessImplementation : GeneralOptionsTabHarness
        {
            public GOTabHarnessImplementation(GeneralOptionsViewModel viewModel)
            {
                InitializeComponent();
                GeneralOptionsTab.DataContext = viewModel;
            }


            public override TabItem GetTabItem()
            {
                var tab = GeneralOptionsTab;
                RootTabs.Items.Clear();
                return tab;
            }
        }
    }
}