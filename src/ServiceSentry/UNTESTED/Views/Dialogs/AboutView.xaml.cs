// -----------------------------------------------------------------------
//  <copyright file="AboutView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Extensibility.Controls;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for AboutView.xaml
    /// </summary>
    public abstract partial class AboutView
    {
        public abstract AboutViewModel ViewModel { get; }

        internal static AboutView GetInstance(AboutViewModel viewModel)
        {
            return new AboutViewImplementation(viewModel);
        }

        protected abstract void OnOkClick(object sender, RoutedEventArgs e);
        protected abstract void BackgroundBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e);
        protected abstract void GridColumnHeaderClick(object sender, RoutedEventArgs e);
        protected abstract void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e);

        private sealed class AboutViewImplementation : AboutView
        {
            private readonly AboutViewModel _viewModel;

            internal AboutViewImplementation(AboutViewModel viewModel)
            {
                _viewModel = viewModel;
                InitializeComponent();
                InstalledEnginesListView.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(this.Thumb_DragDelta),
                                                 true);
                //this.ResizeColumns(InstalledEnginesGridView);
            }

            public override AboutViewModel ViewModel
            {
                get { return _viewModel; }
            }

            protected override void OnOkClick(object sender, RoutedEventArgs e)
            {
                Close();
            }

            protected override void BackgroundBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                OnMouseLeftButtonDown(e);
                DragMove();
            }

            protected override void GridColumnHeaderClick(object sender, RoutedEventArgs e)
            {
                this.GridViewColumnHeaderClicked(sender, e);
            }

            protected override void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
        }
    }
}