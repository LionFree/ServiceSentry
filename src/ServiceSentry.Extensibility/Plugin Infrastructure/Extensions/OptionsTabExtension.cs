// -----------------------------------------------------------------------
//  <copyright file="OptionsTabExtension.cs" company="Curtis Kaler">
//      Copyright (c) 2013 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows.Controls;
using ServiceSentry.Extensibility.Interfaces;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Extensibility.Extensions
{
    public abstract class OptionsTabExtension : PropertyChangedBase, IOptionsTabExtension
    {
        public abstract string ExtensionName { get; }
        
        public abstract bool CanExecute { get; }
        public abstract void CommitOptions();

        public virtual void RefreshOptionSettings()
        {
        }

        public virtual void OnImportsSatisfied()
        {
        }

        public abstract TabItem OptionTabItem(Logger logger);
    }
}