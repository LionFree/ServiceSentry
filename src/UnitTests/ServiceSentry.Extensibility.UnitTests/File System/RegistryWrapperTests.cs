// -----------------------------------------------------------------------
//  <copyright file="WindowsRegistryTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using Microsoft.Win32;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.File_System
{
    [TestFixture]
    internal class RegistryTests
    {
        [Test]
        public void Test_Registry_CurrentUser()
        {
            // Arrange
            var expected = RegistryKeyWrapper.GetInstance(Registry.CurrentUser);
            var sut = WindowsRegistry.Default;

            // Act
            var actual = sut.CurrentUser;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Registry_LocalMachine()
        {
            // Arrange
            var expected = RegistryKeyWrapper.GetInstance(Registry.LocalMachine);
            var sut = WindowsRegistry.Default;

            // Act
            var actual = sut.LocalMachine;

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    internal class RegistryKeyTests
    {
        [SetUp]
        public void SetUp()
        {
            _keyName = Tests.Random<string>();
            _key = Registry.CurrentUser.CreateSubKey(_keyName);
            Assert.IsNotNull(_key);
        }

        [TearDown]
        public void TearDown()
        {
            Registry.CurrentUser.DeleteSubKeyTree(_keyName);
        }

        private string _keyName;
        private RegistryKey _key;

        private void Test_Key_DeleteValue(bool throwsOnMissingValue, bool valueExists)
        {
            // Arrange
            var thrown = false;
            var name = Tests.Random<string>();

            if (valueExists) _key.SetValue(name, Tests.Random<string>());

            var sut = RegistryKeyWrapper.GetInstance(_key);

            // Act
            try
            {
                sut.DeleteValue(name, throwsOnMissingValue);
            }
            catch
            {
                thrown = true;
            }
            finally
            {
                // Assert
                var actual = _key.GetValue(name);
                Assert.IsNull(actual);

                if (valueExists)
                {
                    Assert.IsFalse(thrown);
                }
                else
                {
                    Assert.AreEqual(throwsOnMissingValue, thrown);
                }
            }
        }

        [Test]
        public void Test_Key_Constructor_with_null_key_parameter()
        {
            // Arrange / Act
            var sut = RegistryKeyWrapper.GetInstance(null);

            // Assert
            Assert.IsNull(sut);
        }

        [Test]
        public void Test_Key_CreateSubKey()
        {
            // Arrange
            var keyName = Tests.Random<string>();
            var sut = RegistryKeyWrapper.GetInstance(Registry.CurrentUser);

            // Act
            var actualKey = sut.CreateSubKey(keyName);

            // Assert
            Assert.That(Registry.CurrentUser.GetSubKeyNames().Contains(keyName));
            Registry.CurrentUser.DeleteSubKey(keyName);
            Assert.IsNotNull(actualKey);
        }

        [Test]
        public void Test_Key_CreateSubKey_Disposed()
        {
            // Arrange
            RegistryKeyWrapper actual = null;
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => { actual = sut.CreateSubKey(Tests.Random<string>()); });
            Assert.IsNull(actual);
        }

        [Test]
        public void Test_Key_DeleteValue_Disposed()
        {
            // Arrange
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => sut.DeleteValue(Tests.Random<string>(), true));
            Assert.Throws<ObjectDisposedException>(() => sut.DeleteValue(Tests.Random<string>(), false));
        }

        [Test]
        public void Test_Key_DeleteValue_DoNotThrow()
        {
            Test_Key_DeleteValue(false, true);
            Test_Key_DeleteValue(false, false);
        }

        [Test]
        public void Test_Key_DeleteValue_Throw_MissingValue()
        {
            Test_Key_DeleteValue(true, false);
        }

        [Test]
        public void Test_Key_DeleteValue_Throw_ValueExists()
        {
            Test_Key_DeleteValue(true, true);
        }

        [Test]
        public void Test_Key_EntryExists_Disposed()
        {
            // Arrange
            object actual = null;
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => { actual = sut.EntryExists(Tests.Random<string>()); });
            Assert.IsNull(actual);
        }

        [Test]
        public void Test_Key_EntryExists_False()
        {
            // Arrange
            var sut = RegistryKeyWrapper.GetInstance(_key);

            // Act
            var valueExists = sut.EntryExists(Tests.Random<string>());

            // Assert
            Assert.IsFalse(valueExists);
        }

        [Test]
        public void Test_Key_EntryExists_True()
        {
            // Arrange
            var subkey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control");
            Assert.IsNotNull(subkey);
            var values = subkey.GetValueNames();
            Assert.AreNotEqual(0, values.Length);
            var existingValue = values[Tests.Randomizer.Next(values.Length)];

            var sut = RegistryKeyWrapper.GetInstance(subkey);

            // Act
            var valueExists = sut.EntryExists(existingValue);

            // Assert
            Assert.IsTrue(valueExists);
        }

        [Test]
        public void Test_Key_GetSubKeyNames()
        {
            // Arrange
            var key = Tests.Random<RegistryKey>();
            var expected = key.GetSubKeyNames();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            var actual = sut.GetSubKeyNames();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Key_GetSubKeyNames_Disposed()
        {
            // Arrange
            string[] actual = null;
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => { actual = sut.GetSubKeyNames(); });
            Assert.IsNull(actual);
        }

        [Test]
        public void Test_Key_GetValue()
        {
            // Arrange
            var name = Tests.Random<string>();
            _key.SetValue(name, Tests.Random<string>());
            var expected = _key.GetValue(name);

            var sut = RegistryKeyWrapper.GetInstance(_key);

            // Act

            var actual = sut.GetValue(name);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Key_GetValueNames()
        {
            // Arrange
            var subkeys = Registry.CurrentUser.GetSubKeyNames();
            var subkeyName = subkeys[Tests.Randomizer.Next(subkeys.Length)];
            var subkey = Registry.CurrentUser.OpenSubKey(subkeyName);
            Assert.IsNotNull(subkey);
            var expected = subkey.GetValueNames();
            var sut = RegistryKeyWrapper.GetInstance(subkey);

            // Act
            var actual = sut.GetValueNames();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Key_GetValueNames_Disposed()
        {
            // Arrange
            object actual = null;
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => { actual = sut.GetValueNames(); });
            Assert.IsNull(actual);
        }

        [Test]
        public void Test_Key_GetValue_Disposed()
        {
            // Arrange
            object actual = null;
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => { actual = sut.GetValue(Tests.Random<string>()); });
            Assert.IsNull(actual);
        }

        [Test]
        public void Test_Key_Name()
        {
            // Arrange
            var key = Tests.Random<RegistryKey>();
            var expected = key.Name;
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            var actual = sut.Name;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Key_Name_Disposed()
        {
            // Arrange
            string actual = null;
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => { actual = sut.Name; });
            Assert.IsNull(actual);
        }

        [Test]
        public void Test_Key_ReadSubKey()
        {
            // Arrange
            var subkeys = Registry.CurrentUser.GetSubKeyNames();
            var subkeyName = subkeys[Tests.Randomizer.Next(subkeys.Length)];
            var subkey = Registry.CurrentUser.OpenSubKey(subkeyName, false);
            Assert.IsNotNull(subkey);
            var expected = RegistryKeyWrapper.GetInstance(subkey);
            var sut = RegistryKeyWrapper.GetInstance(Registry.CurrentUser);

            // Act
            var actual = sut.ReadSubKey(subkeyName);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Key_ReadSubKey_Disposed()
        {
            // Arrange
            object actual = null;
            var key = Tests.Random<RegistryKey>();
            var sut = RegistryKeyWrapper.GetInstance(key);

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => { actual = sut.ReadSubKey(Tests.Random<string>()); });
            Assert.IsNull(actual);
        }

        [Test]
        public void Test_Key_SetValue()
        {
            // Arrange
            var name = Tests.Random<string>();
            var expected = Tests.Random<string>();

            var sut = RegistryKeyWrapper.GetInstance(_key);
            sut.SetValue(name, expected);

            // Act
            var actual = _key.GetValue(name);

            Assert.AreEqual(expected, actual, "Value not set correctly.");
        }

        [Test]
        public void Test_Key_SetValue_Disposed()
        {
            // Arrange
            var sut = RegistryKeyWrapper.GetInstance(Tests.Random<RegistryKey>());

            // Act
            sut.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => sut.SetValue(Tests.Random<string>(), Tests.Random<string>()));
        }
    }
}