// -----------------------------------------------------------------------
//  <copyright file="EquatableBase.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Runtime.Serialization;

#endregion

namespace ServiceSentry.Extensibility
{
    /// <summary>
    ///     A simple base class that provides an implementation of the
    ///     <see cref="System.IEquatable&lt;Object&gt;" /> pattern on top of the
    ///     <see cref="T:System.ComponentModel.INotifyPropertyChanged" />
    ///     implementation of the
    ///     <see cref="T:ServiceSentry.Extensibility.PropertyChangedBase" /> base class.
    /// </summary>
    [DataContract]
    public abstract class EquatableBase : PropertyChangedBase, IEquatable<object>
    {
        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();

        public static bool operator ==(EquatableBase left, EquatableBase right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (((object) left) == null || ((object) right) == null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(EquatableBase left, EquatableBase right)
        {
            return !(left == right);
        }
    }
}