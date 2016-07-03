// -----------------------------------------------------------------------
//  <copyright file="RegistryBehaviorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Behavior
{
    [TestFixture]
    internal class RegistryBehaviorTests
    {
        private readonly Mock<RegistryHelper> _helper;
        private readonly WindowsRegistry _realRegistry;

        public RegistryBehaviorTests()
        {
            _realRegistry = WindowsRegistry.Default;
            _helper = new Mock<RegistryHelper>();
        }

        [Test]
        public void Test_RegistryBehavior_GetInstallLocation()
        {
            // Arrange
            var expected = Tests.Random<string>();
            var possibleValues = new List<string>
                {
                    Tests.Random<string>(),
                    expected,
                    Tests.Random<string>()
                };
            _helper.Setup(m => m.GetMatchingValue(It.IsAny<RegistryKeyWrapper>(), possibleValues, "InstallLocation"))
                   .Returns(expected);
            var sut = RegistryBehavior.GetInstance(_realRegistry, _helper.Object);

            // Act
            var actual = sut.GetInstallLocation(possibleValues);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_RegistryBehavior_GetServiceName()
        {
            // Arrange
            var expected = Tests.Random<string>();
            var partial = Tests.Random<string>();

            _helper.Setup(m => m.GetKeyNameFromPartialValueMatch(It.IsAny<RegistryKeyWrapper>(), "ImagePath", partial))
                   .Returns(expected);
            var sut = RegistryBehavior.GetInstance(_realRegistry, _helper.Object);

            // Act
            var actual = sut.GetServiceName(partial);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_RegistryBehavior_InstalledApps()
        {
            // Arrange
            var cu32WasCalled = false;
            var lm32WasCalled = false;
            var lm64WasCalled = false;

            var cu32 = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>()),
                    new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>())
                };
            var lm32 = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>()),
                    new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>())
                };
            var lm64 = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>()),
                    new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>())
                };
            var expected = new List<Tuple<string, string>>();
            expected.AddRange(cu32);
            expected.AddRange(lm32);
            expected.AddRange(lm64);

            _helper.Setup(
                m =>
                m.ApplicationVersions(_realRegistry.CurrentUser, RegistryBehavior.Uninstall32Key,
                                      RegistryBehavior.ApplicationNameStrings))
                   .Callback(() => { cu32WasCalled = true; }).Returns(cu32);

            _helper.Setup(
                m =>
                m.ApplicationVersions(_realRegistry.LocalMachine, RegistryBehavior.Uninstall32Key,
                                      RegistryBehavior.ApplicationNameStrings))
                   .Callback(() => { lm32WasCalled = true; }).Returns(lm32);

            _helper.Setup(
                m =>
                m.ApplicationVersions(_realRegistry.LocalMachine, RegistryBehavior.Uninstall64Key,
                                      RegistryBehavior.ApplicationNameStrings))
                   .Callback(() => { lm64WasCalled = true; }).Returns(lm64);


            var sut = RegistryBehavior.GetInstance(_realRegistry, _helper.Object);

            // Act
            var actual = sut.InstalledApps;

            // Assert
            Assert.IsTrue(cu32WasCalled);
            Assert.IsTrue(lm32WasCalled);
            Assert.IsTrue(lm64WasCalled);

            Assert.AreEqual(expected, actual);
        }
    }


    [TestFixture]
    internal class RegistryHelperTests
    {
        private void Test_DisplayNameContainsString(bool isOnList, bool hasDisplayName,
                                                    bool hasDisplayVersion)
        {
            // Arrange
            var value = Tests.Random<string>();
            var key = new Mock<RegistryKeyWrapper>();
            key.Setup(m => m.EntryExists("DisplayName")).Returns(hasDisplayName);
            key.Setup(m => m.EntryExists("DisplayVersion")).Returns(hasDisplayVersion);
            key.Setup(m => m.GetValue("DisplayName")).Returns(value);

            // hide the value
            var strings = new List<string>();
            strings.AddRange(JunkStrings());
            if (isOnList) strings.Add(value);
            strings.AddRange(JunkStrings());

            var sut = VersionFromListHelper.GetInstance();

            // Act
            var actual = sut.DisplayNameContainsString(key.Object, strings);

            // Assert
            Assert.AreEqual((hasDisplayName && hasDisplayVersion) && isOnList, actual);
        }

        private static IEnumerable<string> JunkStrings()
        {
            var output = new List<string>();
            for (var i = 0; i < Tests.Random<int>(5, 10); i++)
            {
                output.Add(Tests.Random<string>());
            }
            return output;
        }

        private void Test_GetMatchingValue(bool isOnList)
        {
            // Arrange
            var subkey = new Mock<RegistryKeyWrapper>();
            var value = Tests.Random<string>();
            var expected = isOnList ? Tests.Random<string>() : string.Empty;
            var keyName = Tests.Random<string>();
            var keyNameHider = Tests.Random<string>() + keyName + Tests.Random<string>();

            // hide the name in a pile of them
            var possibleKeys = new List<string>();
            possibleKeys.AddRange(JunkStrings());
            possibleKeys.Add(keyName);
            possibleKeys.AddRange(JunkStrings());

            // hide the key in a pile of them
            var existingKeysList = new List<string>();
            existingKeysList.AddRange(JunkStrings());
            if (isOnList)
            {
                existingKeysList.Add(keyNameHider);
                subkey.Setup(m => m.GetValue(value)).Returns(expected);
            }
            existingKeysList.AddRange(JunkStrings());
            var existingKeys = existingKeysList.ToArray();

            var key = new Mock<RegistryKeyWrapper>();
            key.Setup(m => m.GetSubKeyNames()).Returns(existingKeys);
            key.Setup(m => m.ReadSubKey(It.IsAny<string>())).Returns(subkey.Object);


            var sut = RegistryHelper.GetInstance();

            // Act
            var actual = sut.GetMatchingValue(key.Object, possibleKeys, value);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        public void Test_GetKeyNameFromPartialValueMatch(bool isOnList)
        {
            // Arrange
            var expected = isOnList ? Tests.Random<string>() : string.Empty;
            var entryName = Tests.Random<string>();
            var valueToMatch = Tests.Random<string>();
            var key = new Mock<RegistryKeyWrapper>();

            // Give the subkeys names
            var listOfNames = new List<string>();
            listOfNames.AddRange(JunkStrings());
            listOfNames.AddRange(JunkStrings());

            // pick a place to put the winner, and put the winner's name there
            var index = Tests.Random<int>(listOfNames.Count);
            if (isOnList)
            {
                Trace.WriteLine("Expected: " + expected);
                listOfNames[index] = expected;
            }
            var names = listOfNames.ToArray();

            // Create a list of subkeys, hiding the value in there, if applicable
            var valueHider = Tests.Random<string>() + (isOnList ? valueToMatch : Tests.Random<string>());
            var output = new List<Mock<RegistryKeyWrapper>>();
            for (var i = 0; i < names.Length; i++)
            {
                var subkey = new Mock<RegistryKeyWrapper>();
                subkey.Setup(m => m.Name).Returns(names[i]);
                subkey.Setup(m => m.GetValue(entryName)).Returns(i == index ? valueHider : Tests.Random<string>());
                output.Add(subkey);
            }
            var subkeys = output.ToArray();

            // setup key to return each subkey
            key.Setup(m => m.GetSubKeyNames()).Returns(names);
            foreach (var item in subkeys)
            {
                var item1 = item;
                key.Setup(m => m.ReadSubKey(item1.Object.Name)).Returns(item1.Object);
            }

            var sut = RegistryHelper.GetInstance();

            // Act
            var actual = sut.GetKeyNameFromPartialValueMatch(key.Object, entryName, valueToMatch);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        private void Test_GetVersionFromKey(bool hasDisplayName, bool hasDisplayVersion)
        {
            // Arrange
            Tuple<string, string> expected = null;
            Tuple<string, string> actual = null;

            var key = new Mock<RegistryKeyWrapper>();
            key.Setup(m => m.EntryExists("DisplayName")).Returns(hasDisplayName);
            key.Setup(m => m.EntryExists("DisplayVersion")).Returns(hasDisplayVersion);
            if (hasDisplayName && hasDisplayVersion)
            {
                expected = new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>());
                key.Setup(m => m.GetValue("DisplayName")).Returns(expected.Item1);
                key.Setup(m => m.GetValue("DisplayVersion")).Returns(expected.Item2);
            }

            var sut = VersionFromListHelper.GetInstance();

            // Act / Assert
            if (!(hasDisplayName && hasDisplayVersion))
            {
                Assert.Throws<ArgumentException>(() => { actual = sut.VersionFromKey(key.Object); });
            }

            if (hasDisplayName && hasDisplayVersion) actual = sut.VersionFromKey(key.Object);
            Assert.AreEqual(expected, actual);
        }

        public void Test_VersionFromList(bool isOnList)
        {
            // Arrange
            Tuple<string, string> expected = null;
            var list = new List<string>();
            list.AddRange(JunkStrings());

            var key = isOnList ? new Mock<RegistryKeyWrapper>().Object : null;

            var helper = new Mock<VersionFromListHelper>();
            helper.Setup(m => m.DisplayNameContainsString(key, list)).Returns(isOnList);

            if (isOnList)
            {
                expected = new Tuple<string, string>(Tests.Random<string>(), Tests.Random<string>());
                helper.Setup(m => m.VersionFromKey(key)).Returns(expected);
            }


            var sut = VersionHelper.GetInstance(helper.Object);

            // Act
            var actual = sut.VersionFromList(key, list);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        private Tuple<string, string> GetTuple()
        {
            if (Tests.Random<bool>()) return null;

            var item1 = Tests.Random<string>();
            var item2 = Tests.Random<string>();

            Trace.WriteLine(String.Format("Item ({0}:{1}) is expected.", item1, item2));
            return new Tuple<string, string>(item1, item2);
        }

        private string[] CreateNames()
        {
            // Create a list of subkey names
            var listOfNames = new List<string>();
            listOfNames.AddRange(JunkStrings());
            listOfNames.AddRange(JunkStrings());
            return listOfNames.ToArray();
        }

        private void ShowList(IEnumerable<Tuple<string, string>> items)
        {
            foreach (var item in items)
            {
                Trace.WriteLine(String.Format("{0}:{1}", item.Item1, item.Item2));
            }
        }

        public Tuple<RegistryKeyWrapper, List<Tuple<string, string>>> GetKey(string[] names,
                                                                                  List<string> strings = null,
                                                                                  Mock<VersionHelper> helper = null)
        {
            var key = new Mock<RegistryKeyWrapper>();
            key.Setup(m => m.GetSubKeyNames()).Returns(names);

            var expected = new List<Tuple<string, string>>();

            // Create respective subkeys.  Put tuples on some of them
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var subkey = new Mock<RegistryKeyWrapper>();
                subkey.Setup(m => m.Name).Returns(names[i]);

                var tuple = GetTuple();
                if (tuple != null) expected.Add(tuple);

                key.Setup(m => m.ReadSubKey(name)).Returns(subkey.Object);

                if (helper != null)
                {
                    helper.Setup(m => m.VersionFromList(It.Is<RegistryKeyWrapper>(w => w.Name == name), strings))
                          .Returns(tuple);
                }
            }
            Trace.WriteLine(String.Format("Expected count: {0}/{1}", expected.Count, names.Length));

            return new Tuple<RegistryKeyWrapper, List<Tuple<string, string>>>(key.Object, expected);
        }

        [Test]
        public void Test_ApplicationVersionHelper_GetVersionsFromSubkeys()
        {
            // Arrange
            var helper = new Mock<VersionHelper>();
            var strings = RegistryBehavior.ApplicationNameStrings;

            var names = CreateNames();
            var obj = GetKey(names, strings, helper);

            var key = obj.Item1;
            var expected = obj.Item2;

            var sut = ApplicationVersionHelper.GetInstance(helper.Object);

            // Act
            var actual = sut.GetVersionsFromSubkeys(key, names, strings);

            ShowList(actual);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_RegistryHelper_ApplicationVersions()
        {
            // Arrange
            var appVersionHelper = new Mock<ApplicationVersionHelper>();
            var strings = RegistryBehavior.ApplicationNameStrings;
            var names = CreateNames();

            var obj = GetKey(names);
            var key = obj.Item1;
            var expected = obj.Item2;

            appVersionHelper.Setup(m => m.GetVersionsFromSubkeys(key, names, strings)).Returns(expected);

            var registryHive = new Mock<RegistryKeyWrapper>();
            registryHive.Setup(m => m.ReadSubKey(It.IsAny<string>())).Returns(key);

            var sut = RegistryHelper.GetInstance(appVersionHelper.Object);

            // Act
            var actual = sut.ApplicationVersions(registryHive.Object, Tests.Random<string>(), strings);

            ShowList(actual);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_RegistryHelper_GetKeyNameFromPartialValueMatch_Golden_NotOnList()
        {
            Test_GetKeyNameFromPartialValueMatch(false);
        }

        [Test]
        public void Test_RegistryHelper_GetKeyNameFromPartialValueMatch_Golden_OnList()
        {
            Test_GetKeyNameFromPartialValueMatch(true);
        }

        [Test]
        public void Test_RegistryHelper_GetMatchingValue_NotOnList()
        {
            Test_GetMatchingValue(false);
        }

        [Test]
        public void Test_RegistryHelper_GetMatchingValue_OnList()
        {
            Test_GetMatchingValue(true);
        }

        [Test]
        public void Test_VersionFromListHelper_DisplayNameContainsString_Golden_NotOnList()
        {
            Test_DisplayNameContainsString(false, true, true);
        }

        [Test]
        public void Test_VersionFromListHelper_DisplayNameContainsString_Golden_OnList()
        {
            Test_DisplayNameContainsString(true, true, true);
        }

        [Test]
        public void Test_VersionFromListHelper_DisplayNameContainsString_NoDisplayName()
        {
            Test_DisplayNameContainsString(true, false, true);
        }

        [Test]
        public void Test_VersionFromListHelper_DisplayNameContainsString_NoVersion()
        {
            Test_DisplayNameContainsString(true, true, false);
        }

        [Test]
        public void Test_VersionFromListHelper_GetVersionFromKey_Golden()
        {
            Test_GetVersionFromKey(true, true);
        }

        [Test]
        public void Test_VersionFromListHelper_GetVersionFromKey_NoDisplayName()
        {
            Test_GetVersionFromKey(false, true);
        }

        [Test]
        public void Test_VersionFromListHelper_GetVersionFromKey_NoDisplayVersion()
        {
            Test_GetVersionFromKey(true, false);
        }

        [Test]
        public void Test_VersionHelper_VersionFromList_NotOnList()
        {
            Test_VersionFromList(false);
        }

        [Test]
        public void Test_VersionHelper_VersionFromList_OnList()
        {
            Test_VersionFromList(true);
        }

        [Test]
        public void Test_VersionHelper_VersionFromList_key_is_null()
        {
            // Arrange
            var list = new List<string>();
            list.AddRange(JunkStrings());
            var sut = VersionHelper.GetInstance();

            // Act
            var actual = sut.VersionFromList(null, list);

            // Assert
            Assert.IsNull(actual);
        }
    }
}