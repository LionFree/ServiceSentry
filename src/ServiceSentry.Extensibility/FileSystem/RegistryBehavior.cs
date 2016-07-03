using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceSentry.Extensibility
{
    public abstract class RegistryBehavior
    {
        internal const string Uninstall64Key = @"SOFTWARE\WoW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        internal static string Uninstall32Key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        internal static string ServicesKeyString = "SYSTEM\\CurrentControlSet\\services";
        internal static List<string> ApplicationNameStrings = new List<string> {"Curtis Kaler", "Symyx", "EKB"};

        #region Abstract Members

        /// <summary>
        ///     Returns an ObservableCollection &lt;string name, string version&gt; of all installed applications.
        /// </summary>
        public abstract List<Tuple<string, string>> InstalledApps { get; }

        /// <summary>
        ///     Searches the registry uninstall keys, and returns the
        ///     install location of an application, given a list of the
        ///     application's potential names (as they would appear in
        ///     the Windows registry uninstall table).
        /// </summary>
        /// <param name="keysToFind">A list of potential application names to find.</param>
        /// <returns>
        ///     The install location of the first match from the Windows
        ///     registry uninstall table.
        /// </returns>
        public abstract string GetInstallLocation(List<string> keysToFind);

        /// <summary>
        ///     Searches for part of an ImagePath in the Windows registry
        ///     installed services table, and returns the
        ///     <see cref="System.ServiceProcess.ServiceController.ServiceName" />
        ///     of the first match.
        /// </summary>
        /// <param name="partialPath">The partial ImagePath to search for.</param>
        /// <returns>
        ///     The <see cref="System.ServiceProcess.ServiceController.ServiceName" /> of the first match.
        /// </returns>
        public abstract string GetServiceName(string partialPath);

        #endregion

        public static RegistryBehavior GetInstance()
        {
            return GetInstance(WindowsRegistry.Default, RegistryHelper.GetInstance());
        }

        internal static RegistryBehavior GetInstance(WindowsRegistry registry, RegistryHelper helper)
        {
            return new RegistryBehaviorImplementation(registry, helper);
        }

        private sealed class RegistryBehaviorImplementation : RegistryBehavior
        {
            private readonly RegistryHelper _helper;
            private readonly WindowsRegistry _registry;

            public RegistryBehaviorImplementation(WindowsRegistry registry, RegistryHelper helper)
            {
                _registry = registry;
                _helper = helper;
            }

            public override List<Tuple<string, string>> InstalledApps
            {
                get
                {
                    var items = new List<Tuple<string, string>>();

                    items.AddRange(_helper.ApplicationVersions(_registry.CurrentUser, Uninstall32Key,
                                                               ApplicationNameStrings));
                    items.AddRange(_helper.ApplicationVersions(_registry.LocalMachine, Uninstall32Key,
                                                               ApplicationNameStrings));
                    items.AddRange(_helper.ApplicationVersions(_registry.LocalMachine, Uninstall64Key,
                                                               ApplicationNameStrings));

                    return items;
                }
            }

            public override string GetInstallLocation(List<string> possibleValues)
            {
                var registryKey = _registry.LocalMachine.ReadSubKey(Uninstall32Key) ??
                                  _registry.LocalMachine.ReadSubKey(Uninstall64Key);

                if (registryKey == null) throw new Exception("Cannot find uninstall keys in registry.");

                return _helper.GetMatchingValue(registryKey, possibleValues, "InstallLocation");
            }

            public override string GetServiceName(string partialPath)
            {
                var servicesKey = _registry.LocalMachine.ReadSubKey(ServicesKeyString);
                if (servicesKey == null) throw new Exception("Cannot find services keys in registry.");

                return _helper.GetKeyNameFromPartialValueMatch(servicesKey, "ImagePath", partialPath);
            }
        }
    }

    internal abstract class RegistryHelper
    {
        internal static RegistryHelper GetInstance()
        {
            return GetInstance(ApplicationVersionHelper.GetInstance());
        }

        internal static RegistryHelper GetInstance(ApplicationVersionHelper helper)
        {
            return new RegistryHelperImplementation(helper);
        }

        #region Abstract Members

        /// <summary>
        ///     Searches the specified registry uninstall node for applications
        ///     named in the list of strings, and returns a collection of their
        ///     names and versions.
        /// </summary>
        /// <param name="registryHive">The registry hive to search.</param>
        /// <param name="uninstallKey">The uninstallkey to search.</param>
        /// <param name="strings">The list of strings to match.</param>
        /// <returns>A collection of application names and their versions.</returns>
        internal abstract List<Tuple<string, string>> ApplicationVersions(
            RegistryKeyWrapper registryHive, string uninstallKey, List<string> strings);

        /// <summary>
        ///     Inspects the subkeys of a registry key, searching for the name of a specific entry whose value matches a partial value.
        /// </summary>
        /// <param name="key">The registry key to search.</param>
        /// <param name="entryName">The name of the value to compare.</param>
        /// <param name="valueToMatch">The partial value to find.</param>
        /// <returns>The entry name whose value matches the partial value.</returns>
        internal abstract string GetKeyNameFromPartialValueMatch(RegistryKeyWrapper key, string entryName,
                                                                 string valueToMatch);

        /// <summary>
        ///     Given a registry key, searches for a subkey with a name that contains one of
        ///     a list of possible values.
        ///     Returns a specified value if it finds a subkey with such a name,
        ///     otherwise returns <see cref="string.Empty" />.
        /// </summary>
        /// <param name="key">The registry key to search.</param>
        /// <param name="possibleKeyNames">A list of possible key names.</param>
        /// <param name="valueToReturn">The name of the value to return.</param>
        /// <returns>The specified value from a subkey whose name matches an entry from a list of possible values.</returns>
        internal abstract string GetMatchingValue(RegistryKeyWrapper key, List<string> possibleKeyNames,
                                                  string valueToReturn);

        #endregion

        private sealed class RegistryHelperImplementation : RegistryHelper
        {
            private readonly ApplicationVersionHelper _helper;

            internal RegistryHelperImplementation(ApplicationVersionHelper helper)
            {
                _helper = helper;
            }

            internal override List<Tuple<string, string>> ApplicationVersions(
                RegistryKeyWrapper registryHive, string uninstallKey, List<string> strings)
            {
                var collection = new List<Tuple<string, string>>();

                using (var key = registryHive.ReadSubKey(uninstallKey))
                {
                    if (key != null)
                    {
                        // Get the subkeys (the items in the uninstall key)
                        var subKeyNames = key.GetSubKeyNames();

                        collection.AddRange(_helper.GetVersionsFromSubkeys(key, subKeyNames, strings));
                    }
                }

                return collection;
            }

            internal override string GetKeyNameFromPartialValueMatch(RegistryKeyWrapper key, string entryName,
                                                                     string valueToMatch)
            {
                var collection = key.GetSubKeyNames();

                // for each subkey name
                foreach (var keyName in collection)
                {
                    // open that subkey
                    var keyToInspect = key.ReadSubKey(keyName);
                    if (keyToInspect == null) continue;

                    // get the specified value
                    var value = keyToInspect.GetValue(entryName) as string;
                    if (value == null) continue;

                    // if the value matches, return the key name
                    if (value.Contains(valueToMatch))
                        return keyName;
                }
                return String.Empty;
            }

            internal override string GetMatchingValue(RegistryKeyWrapper key, List<string> possibleKeyNames,
                                                      string valueToReturn)
            {
                var collection = key.GetSubKeyNames();

                // for each subkey name
                foreach (var subkeyName in collection)
                {
                    // for each possibility
                    foreach (var possibleName in possibleKeyNames)
                    {
                        // if the name of the subkey contains one of the possibilities
                        if (!subkeyName.Contains(possibleName)) continue;

                        // return the indicated value
                        return key.ReadSubKey(subkeyName).GetValue(valueToReturn) as string;
                    }
                }
                return string.Empty;
            }
        }
    }
    
    internal abstract class ApplicationVersionHelper
    {
        internal static ApplicationVersionHelper GetInstance()
        {
            return GetInstance(VersionHelper.GetInstance());
        }

        internal static ApplicationVersionHelper GetInstance(VersionHelper helper)
        {
            return new AppVersionHelperImplementation(helper);
        }

        /// <summary>
        ///     Searches a list of subkeys for applications named
        ///     in the list of strings, and returns a collection
        ///     of their names and versions.
        /// </summary>
        /// <param name="key">The registry key to which the subkeys belong.</param>
        /// <param name="subKeyNames">A list of subkeys to search.</param>
        /// <param name="strings">The list of strings to match.</param>
        /// <returns>A collection of application names and their versions.</returns>
        internal abstract List<Tuple<string, string>> GetVersionsFromSubkeys(RegistryKeyWrapper key,
                                                                                  string[] subKeyNames,
                                                                                  List<string> strings);

        private sealed class AppVersionHelperImplementation : ApplicationVersionHelper
        {
            private readonly VersionHelper _helper;

            internal AppVersionHelperImplementation(VersionHelper helper)
            {
                _helper = helper;
            }

            internal override List<Tuple<string, string>> GetVersionsFromSubkeys(RegistryKeyWrapper key,
                                                                                      string[] subKeyNames,
                                                                                      List<string> strings)
            {
                var collection = new List<Tuple<string, string>>();

                // Iterate through the given subkey names...
                foreach (var subkeyName in subKeyNames)
                {
                    // open each individual subkey...
                    using (var subKey = key.ReadSubKey(subkeyName))
                    {
                        // get the Version info from the list of strings
                        var tuple = _helper.VersionFromList(subKey, strings);

                        // if it came back, add it to the list
                        if (tuple != null) collection.Add(tuple);
                    }
                }

                return collection;
            }
        }
    }

    internal abstract class VersionHelper
    {
        internal static VersionHelper GetInstance()
        {
            return GetInstance(VersionFromListHelper.GetInstance());
        }

        internal static VersionHelper GetInstance(VersionFromListHelper helper)
        {
            return new VersionHelperImplementation(helper);
        }

        /// <summary>
        ///     Given a registry key, returns the DisplayName and DisplayVersion
        ///     values if the DisplayName contains any entry from the list of strings.
        /// </summary>
        /// <param name="key">The registry key to inspect.</param>
        /// <param name="strings">The list of strings to match.</param>
        /// <returns>
        ///     Returns the DisplayName and DisplayVersion values if the DisplayName contains
        ///     any entry from the list of strings.
        ///     Returns <c>null</c> if the key is null or if either of the DisplayName
        ///     or DisplayVersion values do not exist.
        /// </returns>
        internal abstract Tuple<string, string> VersionFromList(RegistryKeyWrapper key, List<string> strings);

        private sealed class VersionHelperImplementation : VersionHelper
        {
            private readonly VersionFromListHelper _helper;

            internal VersionHelperImplementation(VersionFromListHelper helper)
            {
                _helper = helper;
            }

            internal override Tuple<string, string> VersionFromList(RegistryKeyWrapper key, List<string> strings)
            {
                if (key == null) return null;
                return _helper.DisplayNameContainsString(key, strings) ? _helper.VersionFromKey(key) : null;
            }
        }
    }

    internal abstract class VersionFromListHelper
    {
        internal static VersionFromListHelper GetInstance()
        {
            return new VersionFromListHelperImplementation();
        }

        /// <summary>
        ///     Given a registry key, returns the DisplayName and DisplayVersion values.
        /// </summary>
        /// <param name="key">The registry key to inspect.</param>
        /// <returns>The DisplayName and DisplayVersion values.</returns>
        /// <exception cref="ArgumentException">
        ///     If the DisplayName or DisplayVersion values do not exist on the key.
        /// </exception>
        internal abstract Tuple<string, string> VersionFromKey(RegistryKeyWrapper key);

        /// <summary>
        ///     Returns a value indicating whether the DisplayName value in the specified key
        ///     contains any of the entries on the list of strings.
        /// </summary>
        /// <param name="key">
        ///     The <see cref="RegistryKeyWrapper" /> to inspect.
        /// </param>
        /// <param name="strings">A list of possible strings to match.</param>
        /// <returns>
        ///     <c>true</c> if the DisplayName value contains any of the entries
        ///     on the list of strings, otherwise <c>false</c>.  Also returns
        ///     <c>false</c> if DisplayName does not exist in the key.
        /// </returns>
        internal abstract bool DisplayNameContainsString(RegistryKeyWrapper key, List<string> strings);

        private sealed class VersionFromListHelperImplementation : VersionFromListHelper
        {
            internal override Tuple<string, string> VersionFromKey(RegistryKeyWrapper key)
            {
                if (!key.EntryExists("DisplayName"))
                {
                    throw new ArgumentException("key does not contain DisplayName value.");
                }

                if (!key.EntryExists("DisplayVersion"))
                {
                    throw new ArgumentException("key does not contain DisplayVersion value.");
                }

                return new Tuple<string, string>(
                    key.GetValue("DisplayName").ToString(),
                    key.GetValue("DisplayVersion").ToString());
            }

            internal override bool DisplayNameContainsString(RegistryKeyWrapper key, List<string> strings)
            {
                if (!(key.EntryExists("DisplayName") && key.EntryExists("DisplayVersion"))) return false;

                var name = key.GetValue("DisplayName").ToString().ToUpper();
                return strings.Any(s => name.Contains(s.ToUpper()));
            }
        }
    }
}