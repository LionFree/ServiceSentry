// -----------------------------------------------------------------------
//  <copyright file="ResourceManagerWrapper.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Globalization;
using System.Reflection;
using System.Resources;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Model.UNTESTED
{
    /// <summary>
    ///     Resource Manager exposes an assembly's resources to an application for
    ///     the correct CultureInfo.  An example would be localizing text for a
    ///     user-visible message.  Create a set of resource files listing a name
    ///     for a message and its value, compile them using ResGen, put them in
    ///     an appropriate place (your assembly manifest(?)), then create a Resource
    ///     Manager and query for the name of the message you want.  The Resource
    ///     Manager will use CultureInfo.GetCurrentUICulture() to look
    ///     up a resource for your user's locale settings.
    ///     Users should ideally create a resource file for every culture, or
    ///     at least a meaningful subset.  The filenames will follow the naming
    ///     scheme:
    ///     basename.culture name.resources
    ///     The base name can be the name of your application, or depending on
    ///     the granularity desired, possibly the name of each class.  The culture
    ///     name is determined from CultureInfo's Name property.
    ///     An example file name may be MyApp.en-US.resources for
    ///     MyApp's US English resources.
    /// </summary>
    public abstract class ResourceManagerWrapper : DisposableBase
    {
        public ResourceManagerWrapper GetInstance(string baseName, Assembly assembly)
        {
            return new ResourceManagerImplementation(baseName, assembly);
        }

        /// <summary>
        ///     Looks up a <see cref="string" /> resource value for a particular name.
        ///     Looks in the current thread's <see cref="CultureInfo" />, and if not
        ///     found, all parent  <see cref="CultureInfo" />s.
        /// </summary>
        /// <param name="name">
        ///     The name of the <see cref="string" /> resource to find.
        /// </param>
        public abstract string GetString(string name);

        /// <summary>
        ///     Looks up a <see cref="string" /> resource value for a particular name.
        ///     Looks in the specified <see cref="CultureInfo" />, and if not
        ///     found, all parent <see cref="CultureInfo" />s.
        /// </summary>
        /// <param name="name">
        ///     The name of the <see cref="string" /> resource to find.
        /// </param>
        /// <param name="culture">
        ///     The <see cref="CultureInfo" /> to search.
        /// </param>
        /// <returns>
        ///     The resource <see cref="string" /> if found, otherwise <c>null</c>.
        /// </returns>
        public abstract string GetString(string name, CultureInfo culture);

        /// <summary>
        ///     Looks up an <see cref="object" /> resource value for a particular name.
        ///     Looks in the current thread's <see cref="CultureInfo" />, and if not
        ///     found, all parent <see cref="CultureInfo" />s.
        /// </summary>
        /// <param name="name">
        ///     The name of the <see cref="object" /> resource to find.
        /// </param>
        /// <returns>
        ///     The resource <see cref="object" /> if found, otherwise <c>null</c>.
        /// </returns>
        public abstract object GetObject(string name);

        /// <summary>
        ///     Looks up an <see cref="object" /> resource value for a particular name.
        ///     Looks in the specified <see cref="CultureInfo" />, and if not found,
        ///     all parent <see cref="CultureInfo" />s.
        /// </summary>
        /// <param name="name">
        ///     The name of the <see cref="object" /> resource to find.
        /// </param>
        /// <param name="culture">
        ///     The <see cref="CultureInfo" /> to search.
        /// </param>
        /// <returns>
        ///     The resource <see cref="object" /> if found, otherwise <c>null</c>.
        /// </returns>
        public abstract object GetObject(string name, CultureInfo culture);

        private sealed class ResourceManagerImplementation : ResourceManagerWrapper
        {
            private ResourceManager _resourceManager;

            public ResourceManagerImplementation(string baseName, Assembly assembly)
            {
                _resourceManager = new ResourceManager(baseName, assembly);
            }

            public override string GetString(string name)
            {
                return GetString(name, null);
            }

            public override string GetString(string name, CultureInfo culture)
            {
                return _resourceManager.GetString(name, culture);
            }

            public override object GetObject(string name)
            {
                return GetObject(name, null);
            }

            public override object GetObject(string name, CultureInfo culture)
            {
                return _resourceManager.GetObject(name, culture);
            }

            protected override void Dispose(bool disposing)
            {
                if (!IsDisposed)
                {
                    if (disposing)
                    {
                        if (_resourceManager != null)
                        {
                            _resourceManager.ReleaseAllResources();
                            _resourceManager = null;
                        }
                    }

                    IsDisposed = true;
                    base.Dispose(disposing);
                }
            }
        }
    }
}