// -----------------------------------------------------------------------
//  <copyright file="RelayCommand'2.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public partial class RelayCommand
    {
        private readonly Action<object, object> _execute2;

        public RelayCommand(string name, Action<object, object> execute)
            : this(name, execute, null)
        {
        }

        public RelayCommand(string name, Action<object, object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw new ArgumentException("Command name cannot be an empty string.", "name");

            ProtectedName = name;
            _execute2 = execute;
            ProtectedCanExecute = canExecute;
        }

        public void Execute(object parameter1, object parameter2)
        {
            _execute2(parameter1, parameter2);
        }
    }
}