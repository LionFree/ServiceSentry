// -----------------------------------------------------------------------
//  <copyright file="RelayCommand'1.cs" company="Curtis Kaler">
//      Copyright (c) 2013 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public partial class RelayCommand : RelayCommandBase
    {
        private readonly Action<object> _execute1;

        public RelayCommand(string name, Action<object> execute)
            : this(name, execute, null)
        {
        }

        public RelayCommand(string name, Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw new ArgumentException("Command name cannot be an empty string.", "name");

            ProtectedName = name;
            _execute1 = execute;
            ProtectedCanExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            _execute1(parameter);
        }
    }
}