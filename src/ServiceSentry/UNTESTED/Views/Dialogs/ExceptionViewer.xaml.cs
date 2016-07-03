// -----------------------------------------------------------------------
//  <copyright file="ExceptionViewer.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using ServiceSentry.Client.UNTESTED.ViewModels;
using ServiceSentry.Client.UNTESTED.Views.Helpers;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for ExceptionViewer.xaml
    /// </summary>
    public abstract partial class ExceptionViewer
    {
        public static ExceptionViewer GetInstance()
        {
            return new ExceptionViewerImplementation(ExceptionViewerViewModel.GetInstance());
        }

        internal abstract void ShowException(Exception ex);

        internal abstract void ExceptionViewer_OnSizeChanged(object sender, SizeChangedEventArgs e);
        internal abstract void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e);
        internal abstract void WrapText_OnChecked(object sender, RoutedEventArgs e);
        internal abstract void CopyToClipboard_OnClick(object sender, RoutedEventArgs e);
        internal abstract void ExceptionViewer_OnLoaded(object sender, RoutedEventArgs e);
        internal abstract void OnClosing(object sender, CancelEventArgs e);
        internal abstract void CloseButton_OnClick(object sender, RoutedEventArgs e);

        private sealed class ExceptionViewerImplementation : ExceptionViewer
        {
            private readonly ExceptionViewerHelper _helper;
            private double _chromeWidth;

            public ExceptionViewerImplementation(ExceptionViewerViewModel viewModel)
            {
                DataContext = viewModel;
                InitializeComponent();

                _helper = ExceptionViewerHelper.GetInstance(TreeView.FontSize);
            }

            internal override void ShowException(Exception ex)
            {
                TreeView.Items.Clear();
                _helper.BuildTree(TreeView, ex);
                _helper.ShowCurrentItem(TreeView, DocumentViewer, WrapText.IsChecked);
                Show();
            }

            internal override void ExceptionViewer_OnSizeChanged(object sender, SizeChangedEventArgs e)
            {
                if (e.WidthChanged)
                {
                    _helper.SetMaxColumnWidth(this, _chromeWidth);
                }
            }

            internal override void TreeView_OnSelectedItemChanged(object sender,
                                                                  RoutedPropertyChangedEventArgs<object> e)
            {
                _helper.ShowCurrentItem(TreeView, DocumentViewer, WrapText.IsChecked);
            }

            internal override void WrapText_OnChecked(object sender, RoutedEventArgs e)
            {
                if (!IsInitialized) return;
                _helper.ShowCurrentItem(TreeView, DocumentViewer, WrapText.IsChecked);
            }

            internal override void OnClosing(object sender, CancelEventArgs e)
            {
                Hide();
                e.Cancel = true;
            }

            internal override void CopyToClipboard_OnClick(object sender, RoutedEventArgs e)
            {
                // Build a FlowDocument with Inlines from all top-level tree items.
                var doc = _helper.GetExceptionDetails(TreeView);

                // Now place the doc contents on the clipboard in both
                // rich text and plain text format.
                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                var data = new DataObject();

                using (Stream stream = new MemoryStream())
                {
                    range.Save(stream, DataFormats.Rtf);
                    data.SetData(DataFormats.Rtf, Encoding.UTF8.GetString((stream as MemoryStream).ToArray()));
                }

                data.SetData(DataFormats.StringFormat, range.Text);
                Clipboard.SetDataObject(data);

                // The Inlines that were being displayed are now in the temporary document we just built,
                // causing them to disappear from the viewer.  This puts them back.
                _helper.ShowCurrentItem(TreeView, DocumentViewer, WrapText.IsChecked);
            }

            internal override void ExceptionViewer_OnLoaded(object sender, RoutedEventArgs e)
            {
                // The grid column used for the tree started with Width="Auto" so it is now exactly
                // wide enough to fit the longest exception (up to the MaxWidth set in XAML).
                // Changing the width to a fixed pixel value prevents it from changing if the user
                // resizes the window.

                TreeColumn.Width = new GridLength(TreeColumn.ActualWidth, GridUnitType.Pixel);
                _chromeWidth = ActualWidth - GridRoot.ActualWidth;
                _helper.SetMaxColumnWidth(this, _chromeWidth);
            }

            internal override void CloseButton_OnClick(object sender, RoutedEventArgs e)
            {
                Close();
            }
        }
    }
}