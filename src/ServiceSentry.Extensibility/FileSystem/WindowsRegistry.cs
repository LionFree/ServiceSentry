// -----------------------------------------------------------------------
//  <copyright file="WindowsRegistry.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
using System.IO;

#endregion

namespace ServiceSentry.Extensibility
{
    /// <summary>
    ///     Provides <see cref="T:ServiceSentry.Extensibility.RegistryKeyWrapper" />
    ///     objects that represent the root keys in the Windows registry.
    /// </summary>
    public abstract class WindowsRegistry
    {
        /// <summary>
        ///     Contains information about the current user preferences.
        ///     This field reads the Windows registry base key HKEY_CURRENT_USER.
        /// </summary>
        public abstract RegistryKeyWrapper CurrentUser { get; }

        /// <summary>
        ///     Contains the configuration data for the local machine.
        ///     This field reads the Windows registry base key HKEY_LOCAL_MACHINE.
        /// </summary>
        public abstract RegistryKeyWrapper LocalMachine { get; }

        public static WindowsRegistry Default
        {
            get { return new RegistryImplementation(); }
        }

        private sealed class RegistryImplementation : WindowsRegistry
        {
            public override RegistryKeyWrapper CurrentUser
            {
                get { return RegistryKeyWrapper.GetInstance(Microsoft.Win32.Registry.CurrentUser); }
            }

            public override RegistryKeyWrapper LocalMachine
            {
                get { return RegistryKeyWrapper.GetInstance(Microsoft.Win32.Registry.LocalMachine); }
            }
        }
    }

    /// <summary>
    ///     Represents a key-level node in the Windows registry.
    ///     This class is a registry encapsulation.
    /// </summary>
    public abstract class RegistryKeyWrapper : DisposableEquatableBase
    {
        public static RegistryKeyWrapper GetInstance(Microsoft.Win32.RegistryKey key)
        {
            return key == null ? null : new RegistryKeyWrapperImplementation(key);
        }

        #region Abstract Members

        /// <summary>
        ///     Retrieves the name of the key.
        /// </summary>
        /// <returns>
        ///     The absolute (qualified) name of the key.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">
        ///     The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed).
        /// </exception>
        public abstract string Name { get; }

        /// <summary>
        ///     Creates a new subkey, or opens an existing one.
        /// </summary>
        /// <param name="subkey">Name or path to subkey to create or open.</param>
        /// <returns>
        ///     the subkey, or <b>null</b> if the operation failed.
        /// </returns>
        public abstract RegistryKeyWrapper CreateSubKey(string subkey);

        /// <summary>
        ///     Sets the specified value.
        /// </summary>
        /// <param name="name">Name of value to store data in</param>
        /// <param name="value">Data to store.</param>
        public abstract void SetValue(string name, object value);

        /// <summary>
        ///     Deletes the specified value from this key.
        /// </summary>
        /// <param name="name">Name of the value to delete.</param>
        /// <param name="throwOnMissingValue"></param>
        public abstract void DeleteValue(string name, bool throwOnMissingValue);

        /// <summary>
        ///     Retrieves the value associated with the specified name. Returns null if the name/value pair does not exist in the registry.
        /// </summary>
        /// <returns>
        ///     The value associated with <paramref name="name" />, or null if <paramref name="name" /> is not found.
        /// </returns>
        /// <param name="name">The name of the value to retrieve. </param>
        /// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///     The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value is closed (closed keys cannot be accessed).
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Read="\" />
        /// </PermissionSet>
        public abstract object GetValue(string name);

        /// <summary>
        ///     Retrieves an array of strings that contains all the subkey names.
        /// </summary>
        /// <returns>
        ///     An array of strings that contains the names of the subkeys for the current key.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the key. </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///     The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed).
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        /// <exception cref="T:System.IO.IOException">A system error occurred, for example the current key has been deleted.</exception>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Unrestricted="true" />
        /// </PermissionSet>
        public abstract string[] GetSubKeyNames();

        /// <summary>
        ///     Retrieves an array of strings that contains all the value names associated with this key.
        /// </summary>
        /// <returns>
        ///     An array of strings that contains the value names for the current key.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///     The <see cref="T:Microsoft.Win32.RegistryKey" />  being manipulated is closed (closed keys cannot be accessed).
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        /// <exception cref="T:System.IO.IOException">A system error occurred; for example, the current key has been deleted.</exception>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Unrestricted="true" />
        /// </PermissionSet>
        public abstract string[] GetValueNames();

        /// <summary>
        ///     Retrieves a subkey as read-only.
        /// </summary>
        /// <returns>
        ///     The subkey requested, or null if the operation failed.
        /// </returns>
        /// <param name="name">The name or path of the subkey to open read-only. </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="name" /> is null
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="name" /> is longer than the maximum length allowed (255 characters).
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///     The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed).
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read the registry key. </exception>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Read="\" />
        /// </PermissionSet>
        public abstract RegistryKeyWrapper ReadSubKey(string name);

        /// <summary>
        ///     Determines whether a value exists within this key.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the value exists, otherwise <c>false</c>.
        /// </returns>
        public abstract bool EntryExists(string entryName);

        #endregion

        private sealed class RegistryKeyWrapperImplementation : RegistryKeyWrapper
        {
            private Microsoft.Win32.RegistryKey _key;

            public RegistryKeyWrapperImplementation(Microsoft.Win32.RegistryKey key)
            {
                _key = key;
            }

            public override string Name
            {
                get
                {
                    if (IsDisposed) throw new ObjectDisposedException("_key");
                    return _key.Name;
                }
            }

            public override string[] GetSubKeyNames()
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                return _key.GetSubKeyNames();
            }

            public override RegistryKeyWrapper CreateSubKey(string subkey)
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                return GetInstance(_key.CreateSubKey(subkey));
            }

            public override void SetValue(string name, object value)
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                _key.SetValue(name, value);
            }

            public override void DeleteValue(string name, bool throwOnMissingValue)
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                _key.DeleteValue(name, throwOnMissingValue);
            }

            public override object GetValue(string name)
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                return _key.GetValue(name);
            }

            public override string[] GetValueNames()
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                return _key.GetValueNames();
            }

            public override RegistryKeyWrapper ReadSubKey(string name)
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                return GetInstance(_key.OpenSubKey(name, false));
            }

            public override bool EntryExists(string entryName)
            {
                if (IsDisposed) throw new ObjectDisposedException("_key");
                foreach (var str in GetValueNames())
                {
                    if (str.ToLower() == entryName.ToLower()) return true;
                }
                return false;
            }

            public override bool Equals(object obj)
            {
                // Check for null values and compare run-time types.
                if (obj == null || GetType() != obj.GetType())
                    return false;

                if (ReferenceEquals(this, obj)) return true;

                var p = (RegistryKeyWrapper) obj;

                var sameName = (Name == p.Name);

                var same = (sameName);
                return same;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap.
                {
                    // pick two prime numbers
                    const int seed = 5;
                    var hash = 3;

                    // be sure to check for nullity, etc.
                    hash *= seed + (Name != null ? Name.GetHashCode() : 0);

                    return hash;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (IsDisposed) return;

                if (disposing)
                {
                    if (_key == null) return;
                    try
                    {
                        // release managed resources
                        _key.Close();
                    }
                    catch (IOException ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
                // release unmanaged resources
                _key = null;

                base.Dispose(disposing);
            }
        }
    }
}