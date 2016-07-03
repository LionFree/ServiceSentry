// -----------------------------------------------------------------------
//  <copyright file="AddFileView.xaml.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace ServiceSentry.Client.UNTESTED.Views.Dialogs
{
    /// <summary>
    ///     Interaction logic for AddFileView.xaml
    /// </summary>
    public abstract partial class AddFileView
    {
        internal static AddFileView GetInstance()
        {
            return new AddFileViewImplementation();
        }

        private sealed class AddFileViewImplementation : AddFileView
        {
            internal AddFileViewImplementation()
            {
                InitializeComponent();
            }
        }
    }
}