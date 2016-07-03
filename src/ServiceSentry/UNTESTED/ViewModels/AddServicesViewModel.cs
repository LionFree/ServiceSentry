// -----------------------------------------------------------------------
//  <copyright file="AddServicesViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using System.Windows.Input;
using ServiceSentry.Client.Properties;
using ServiceSentry.Client.UNTESTED.Model;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Controls;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class AddServicesViewModel : PropertyChangedBase
    {
        internal static AddServicesViewModel GetInstance(ApplicationState applicationState, ViewController viewController)
        {
            return new AddServicesVMImplementation(applicationState, viewController);
        }

        #region Abstract Members

        public abstract bool ShowDisplayNameColumn { get; set; }
        public abstract bool ShowServiceNameColumn { get; set; }
        public abstract ObservableCollection<ServiceInfo> Services { get; }
        
        public abstract ICommand AddRemoveServiceCommand { get; }
        public abstract ICommand ColumnSelectedCommand { get; }

        protected abstract void AddRemoveService(object serviceInfo);
        protected abstract void ColumnSelected(object parameter);

        #endregion

        private sealed class AddServicesVMImplementation : AddServicesViewModel
        {
            private readonly ApplicationState _applicationState;
            private readonly ViewController _viewController;
            private ICommand _addRemoveServiceCommand;
            private ICommand _columnSelectedCommand;

            public AddServicesVMImplementation(ApplicationState applicationState, ViewController viewController)
            {
                _applicationState = applicationState;
                _viewController = viewController;
                
                _viewController.ServiceHandler.UpdateListOfAllServices();

                ShowDisplayNameColumn = Settings.Default.ShowDisplayNameColumn;
                ShowServiceNameColumn = Settings.Default.ShowServiceNameColumn;

            }

            public override ObservableCollection<ServiceInfo> Services
            {
                get { return _applicationState.ServiceInfos; }
            }

            public override bool ShowDisplayNameColumn { get; set; }
            public override bool ShowServiceNameColumn { get; set; }

            public override ICommand AddRemoveServiceCommand
            {
                get
                {
                    return _addRemoveServiceCommand ??
                           (_addRemoveServiceCommand = new RelayCommand("AddRemoveService", AddRemoveService));
                }
            }

            public override ICommand ColumnSelectedCommand
            {
                get
                {
                    return _columnSelectedCommand ??
                           (_columnSelectedCommand = new RelayCommand("ColumnSelected", ColumnSelected));
                }
            }

            protected override void AddRemoveService(object serviceInfo)
            {
                var info = serviceInfo as ServiceInfo;
                if (info == null) return;

                info.Selected = _viewController.ServiceHandler.AddOrRemoveServiceSelected(
                    info.ServiceName,
                    _applicationState,
                    _viewController.Behavior, _viewController.Logger);
            }

            protected override void ColumnSelected(object setting)
            {
                var settingName = setting as string;
                if (settingName == null) return;

                var value = (bool) Settings.Default[settingName];
                Settings.Default[settingName] = !value;

                Settings.Default.Save();
            }
        }
    }
}