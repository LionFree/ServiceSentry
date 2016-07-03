// -----------------------------------------------------------------------
//  <copyright file="TrayIconView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Client.UNTESTED.ViewModels;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views
{
    public abstract partial class TrayIconView
    {
        public static TrayIconView GetInstance(TrayIconViewModel viewModel)
        {
            return new TrayIconViewImplementation(viewModel);
        }

        private sealed class TrayIconViewImplementation : TrayIconView
        {
            internal TrayIconViewImplementation(TrayIconViewModel viewModel)
            {
                DataContext = viewModel;
                InitializeComponent();
            }
        }
    }
}