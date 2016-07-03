// -----------------------------------------------------------------------
//  <copyright file="CommandLineUsageViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Windows.Input;
using ServiceSentry.Client.UNTESTED.Views;
using ServiceSentry.Client.UNTESTED.Views.Dialogs;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    internal abstract class CommandLineUsageViewModel : PropertyChangedBase
    {
        internal static CommandLineUsageViewModel GetInstance()
        {
            return GetInstance(CommandLineUsageView.GetInstance());
        }
        internal static CommandLineUsageViewModel GetInstance(CommandLineUsageView view)
        {
            return new CommandLineUsageVMImplementation(view);
        }

        #region Abstract Members

        public abstract string Message { get; }
        public abstract string BadArguments { get; set; }
        public abstract ICommand CloseViewCommand { get; }
        internal abstract void ShowDialog(string[] args);
        internal abstract void CloseView();

        #endregion
        
        private sealed class CommandLineUsageVMImplementation : CommandLineUsageViewModel
        {
            private readonly string _message;
            private readonly CommandLineUsageView _view;
            private string _badArguments;
            private ICommand _closeViewCommand;

            internal CommandLineUsageVMImplementation(CommandLineUsageView view)
            {
                _view = view;
                var name = Strings._ApplicationName;
                var description = Strings._ApplicationDescription;
                var synopsis = name + ".exe " + Strings.CommandLine_Option;

                const string dOption = "--d, --debug";
                var dDetails = Strings.CommandLine_Debug_Details;

                const string mOption = "--m, --minimized";
                var mDetails = Strings.CommandLine_Minimized_Details;

                _message = string.Empty;
                _message += Strings.Noun_Name + Environment.NewLine;
                _message += "\t" + name + Environment.NewLine;
                _message += "\t" + description + Environment.NewLine;
                _message += Environment.NewLine;
                _message += Strings.Noun_Usage + Environment.NewLine;
                _message += "\t" + synopsis + Environment.NewLine;
                _message += Environment.NewLine;
                _message += Strings.Noun_CL_Options + Environment.NewLine;
                _message += "\t" + dOption + Environment.NewLine;
                _message += "\t" + dDetails + Environment.NewLine;
                _message += Environment.NewLine;
                _message += "\t" + mOption + Environment.NewLine;
                _message += "\t" + mDetails + Environment.NewLine;

                _view.DataContext = this;
            }

            public override string Message
            {
                get { return _message; }
            }

            public override string BadArguments
            {
                get { return _badArguments; }
                set
                {
                    if (_badArguments == value) return;
                    _badArguments = value;
                    OnPropertyChanged();
                }
            }
            
            public override ICommand CloseViewCommand
            {
                get
                {
                    return _closeViewCommand ??
                           (_closeViewCommand = new RelayCommand("CloseCLUsageView", m => CloseView()));
                }
            }

            internal override void ShowDialog(string[] args)
            {
                BadArguments = string.Join(" ", args);
                _view.ShowDialog();
            }

            internal override void CloseView()
            {
                _view.Close();
            }
        }
    }
}