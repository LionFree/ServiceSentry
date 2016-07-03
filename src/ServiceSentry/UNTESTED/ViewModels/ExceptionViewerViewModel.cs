// -----------------------------------------------------------------------
//  <copyright file="ExceptionViewerViewModel.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Client.UNTESTED.ViewModels
{
    public abstract class ExceptionViewerViewModel : PropertyChangedBase
    {
        internal static ExceptionViewerViewModel GetInstance()
        {
            return new ExceptionViewerVMImplementation();
        }

        #region Abstract Members

        public abstract string ViewTitle { get; }

        #endregion

        private sealed class ExceptionViewerVMImplementation : ExceptionViewerViewModel
        {
            public override string ViewTitle
            {
                get { return Extensibility.Strings._ApplicationName; }
            }
        }
    }
}