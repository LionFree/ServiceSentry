// -----------------------------------------------------------------------
//  <copyright file="WardenOptionsViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Diagnostics;
using System.Windows.Input;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class WardenOptionsViewModel : PropertyChangedBase
    {
        internal static WardenOptionsViewModel GetInstance(WardenServerDetails server, Logger logger)
        {
            return new WovmImplementation(server);
        }

        #region Abstract Members

        public abstract string HostName { get; set; }
        public abstract int Port { get; set; }
        public abstract bool WardenEnabled { get; set; }
        public abstract string WebAddress { get; protected set; }
        public abstract bool WardenAvailable { get; protected set; }
        
        public abstract ICommand TestWardenCommand { get; }
        public abstract ICommand OpenWardenCommand { get; }

        internal abstract void RefreshSettings(WardenServerDetails server);
        internal abstract WardenServerDetails Commit(WardenServerDetails server);

        #endregion

        private sealed class WovmImplementation : WardenOptionsViewModel
        {
            private string _hostName;
            private int _port;
            private string _webAddress;
            private ICommand _testWardenCommand;
            private bool _wardenAvailable;
            private ICommand _openWardenCommand;

            internal WovmImplementation(WardenServerDetails server)
            {
                RefreshSettings(server);
            }

            public override ICommand TestWardenCommand
            {
                get { return _testWardenCommand ?? (_testWardenCommand = new RelayCommand("TestWardenCommand", param => TestWarden())); }
            }

            public override ICommand OpenWardenCommand
            {
                get { return _openWardenCommand ?? (_openWardenCommand = new RelayCommand("OpenWardenCommand", param => OpenWarden())); }
            }

            private void OpenWarden()
            {
                Trace.WriteLine("Opening Warden in Browser...");
            }

            private void TestWarden()
            {
                Trace.WriteLine("Testing Warden Connection...");
                WardenAvailable = true;
            }

            public override bool WardenEnabled { get; set; }

            public override bool WardenAvailable
            {
                get { return _wardenAvailable; }
                protected set
                {
                    if (_wardenAvailable == value) return;
                    _wardenAvailable = value;
                    OnPropertyChanged();
                }
            }

            public override string WebAddress
            {
                get { return _webAddress; }
                protected set
                {
                    if (_webAddress == value) return;
                    _webAddress = value;
                    OnPropertyChanged();
                }
            }

            public override string HostName
            {
                get { return _hostName; }
                set
                {
                    if (_hostName == value) return;
                    _hostName = value;
                    WardenAvailable = false;
                    UpdateAddress();
                    OnPropertyChanged();
                }
            }
            
            public override int Port
            {
                get { return _port; }
                set
                {
                    if (_port == value) return;
                    _port = value;
                    WardenAvailable = false;
                    UpdateAddress();
                    OnPropertyChanged();
                }
            }

            private void UpdateAddress()
            {
                WebAddress = string.IsNullOrEmpty(HostName)
                    ? Strings.Text_None
                    : HostName.Trim() + ":" + Port + Extensibility.Strings.Warden_BaseAddress;
            }
            
            internal override WardenServerDetails Commit(WardenServerDetails server)
            {
                server.HostName = HostName;
                server.Port = Port;
                
                return server;
            }

            internal override void RefreshSettings(WardenServerDetails server)
            {
                HostName = server.HostName;
                Port = server.Port;
                WardenEnabled = !(string.IsNullOrEmpty(HostName));
            }
        }
    }
}