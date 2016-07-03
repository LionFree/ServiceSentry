// -----------------------------------------------------------------------
//  <copyright file="PropertyChangedBase.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ComponentModel;
using System.Diagnostics;
//using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ServiceSentry.Annotations;

#endregion

#pragma warning disable 1685

namespace ServiceSentry.Extensibility
{
    /// <summary>
    ///     Simple base class that provides a solid implementation
    ///     of the <see cref="T:System.ComponentModel.INotifyPropertyChanged" /> event.
    /// </summary>
    [DataContract]
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler _propertyChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized), DebuggerStepThrough] add { _propertyChanged += value; }
            [MethodImpl(MethodImplOptions.Synchronized), DebuggerStepThrough] remove { _propertyChanged -= value; }
        }

        /// <summary>
        ///     Allows triggering the <see cref="E:ServiceSentry.Extensibility.PropertyChangedBase.PropertyChanged" />
        ///     event using a lambda expression, thus avoiding strings. Keep in
        ///     mind that using this method comes with a performance penalty, so
        ///     don't use it for frequently updated properties that cause a lot
        ///     of events to be fired.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="propertyExpression">
        ///     Expression pointing to a given
        ///     property.
        /// </param>
        public virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            //Contract.Requires(propertyExpression != null);
            OnPropertyChanged(((MemberExpression) propertyExpression.Body).Member.Name);
        }

        /// <summary>
        ///     Raises the <see cref="E:ServiceSentry.Extensibility.PropertyChangedBase.PropertyChanged" />
        ///     event for a given property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        [NotifyPropertyChangedInvocator, DebuggerStepThrough]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Use a variable to prevent race conditions.
            var handler = _propertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:ServiceSentry.Extensibility.PropertyChangedBase.PropertyChanged" />
        ///     event for a set of properties.
        /// </summary>
        /// <param name="propertyNames">Provides the names of the changed properties.</param>
        public void OnPropertyChanged(params string[] propertyNames)
        {
            //Contract.Requires(propertyNames != null);

            foreach (var propertyName in propertyNames)
                OnPropertyChanged(propertyName);
        }
    }
}