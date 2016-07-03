// -----------------------------------------------------------------------
//  <copyright file="OptionsShellView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows;
using ServiceSentry.Client.UNTESTED.ViewModels;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for OptionsShellView.xaml
    /// </summary>
    public abstract partial class OptionsView
    {
        internal static OptionsView GetInstance(OptionsViewModel viewModel)
        {
            return new OptionsViewImplementation(viewModel);
        }

        protected abstract void OnButtonClick(object sender, RoutedEventArgs e);

        private sealed class OptionsViewImplementation : OptionsView
        {
            public OptionsViewImplementation(OptionsViewModel viewModel)
            {
                DataContext = viewModel;
                InitializeComponent();
            }

            protected override void OnButtonClick(object sender, RoutedEventArgs e)
            {
                Tabs.SelectedIndex = 0;
                Close();
            }
        }
    }
}