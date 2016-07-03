// -----------------------------------------------------------------------
//  <copyright file="ViewLocator.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using ServiceSentry.Client.UNTESTED.Views;
using ServiceSentry.Client.UNTESTED.Views.Dialogs;

#endregion

namespace ServiceSentry.Client.UNTESTED.Infrastructure
{
    public abstract class ViewLocator
    {
        internal abstract OptionsView OptionsView { get; }
        internal abstract ShellWindowView ShellView { get; }
        internal abstract AboutView AboutView { get; }

        internal static ViewLocator GetInstance(ViewModelLocator viewModels)
        {
            return new ViewLocatorImplementation(viewModels);
        }


        private sealed class ViewLocatorImplementation : ViewLocator
        {
            private readonly AboutView _aboutView;
            private readonly ShellWindowView _mainView;
            private readonly OptionsView _optionsView;

            internal ViewLocatorImplementation(ViewModelLocator viewModels)
            {
                if(viewModels==null) throw new ArgumentNullException("viewModels");
                
                _mainView = ShellWindowView.GetInstance(viewModels.ShellWindowViewModel);
                _aboutView = AboutView.GetInstance(viewModels.AboutViewModel);
                _optionsView = OptionsView.GetInstance(viewModels.OptionsViewModel);
            }

            internal override AboutView AboutView
            {
                get { return _aboutView; }
            }

            internal override OptionsView OptionsView
            {
                get { return _optionsView; }
            }

            internal override ShellWindowView ShellView
            {
                get { return _mainView; }
            }
        }
    }
}