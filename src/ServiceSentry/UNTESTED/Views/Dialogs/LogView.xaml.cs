// -----------------------------------------------------------------------
//  <copyright file="LogView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for LogView.xaml
    /// </summary>
    public abstract partial class LogView
    {
        internal static LogView GetInstance()
        {
            return new LogViewImplementation(ExceptionViewer.GetInstance());
        }

        internal abstract void GridViewColumnHeaderClick(object sender, RoutedEventArgs e);
        internal abstract void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e);
        internal abstract void OnClosing(object sender, CancelEventArgs e);

        private sealed class LogViewImplementation : LogView
        {
            private readonly ExceptionViewer _viewer;

            internal LogViewImplementation(ExceptionViewer viewer)
            {
                _viewer = viewer;
                InitializeComponent();
            }

            internal override void GridViewColumnHeaderClick(object sender, RoutedEventArgs e)
            {
                this.GridViewColumnHeaderClicked(sender, e);
            }

            internal override void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
            {
                var entry = e.Parameter as LogEntry;
                if (entry == null) return;

                var exception = entry.Exception;
                if (exception == null) return;

                _viewer.ShowException(exception);
            }

            internal override void OnClosing(object sender, CancelEventArgs e)
            {
                Hide();
                e.Cancel = true;
            }
        }
    }
}