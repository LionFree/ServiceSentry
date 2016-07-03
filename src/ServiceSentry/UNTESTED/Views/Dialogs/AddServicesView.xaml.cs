// -----------------------------------------------------------------------
//  <copyright file="AddServicesView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows;
using System.Windows.Controls.Primitives;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Extensibility.Controls;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for AddServicesView.xaml
    /// </summary>
    public abstract partial class AddServicesView
    {

        internal static AddServicesView GetInstance(AddServicesViewModel viewModel)
        {
            return new AddServicesViewImplementation(viewModel);
        }

        #region Abstract Member
        
        public abstract AddServicesViewModel ViewModel { get; }
        protected abstract void OkButton_OnClick(object sender, RoutedEventArgs e);

        #endregion
        
        private sealed class AddServicesViewImplementation : AddServicesView
        {
            private readonly AddServicesViewModel _viewModel;

            internal AddServicesViewImplementation(AddServicesViewModel viewModel)
            {
                _viewModel = viewModel;

                InitializeComponent();
                DataContext = _viewModel;

                InstalledServicesListView.AddHandler(Thumb.DragDeltaEvent,
                                                     new DragDeltaEventHandler(this.Thumb_DragDelta),
                                                     true);
                //this.ResizeColumns(InstalledServicesGridView);
            }

            public override AddServicesViewModel ViewModel
            {
                get { return _viewModel; }
            }

            protected override void OkButton_OnClick(object sender, RoutedEventArgs e)
            {
                Close();
            }
        }
    }
}