// -----------------------------------------------------------------------
//  <copyright file="LoggingDetailsTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.IO;
using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.DataClasses.Serializable
{
    [TestFixture]
    public class LoggingDetailsTests
    {
        private static LoggingDetails CreateTestObject(string path, bool archive, bool clear)
        {
            var output = LoggingDetails.GetInstance();
            output.ArchivePath = path;
            output.ArchiveLogs = archive;
            output.ClearLogs = clear;
            return output;
        }

        [Test]
        public void Test_LoggingDetails_ArchivePathUpdatesOtherProperties()
        {
            var sut = LoggingDetails.GetInstance();

            var testFile = Tests.RandomFilePath();
            var originalPath = sut.ArchivePath;

            sut.ShouldNotNotifyOn(s => s.ArchivePath).When(s => s.ArchivePath = originalPath);
            sut.ShouldNotNotifyOn(s => s.Directory).When(s => s.ArchivePath = originalPath);
            sut.ShouldNotNotifyOn(s => s.FileName).When(s => s.ArchivePath = originalPath);
            sut.ShouldNotNotifyOn(s => s.DisplayName).When(s => s.ArchivePath = originalPath);

            // Should update Directory, FileName, DisplayName
            sut.ShouldNotifyOn(s => s.ArchivePath).When(s => s.ArchivePath = testFile);
            sut.ShouldNotifyOn(s => s.Directory).When(s => s.ArchivePath = originalPath);
            sut.ShouldNotifyOn(s => s.FileName).When(s => s.ArchivePath = testFile);
            sut.ShouldNotifyOn(s => s.DisplayName).When(s => s.ArchivePath = originalPath);
        }

        [Test]
        public void Test_LoggingDetails_ChangingArchiveLogsShouldToggleIgnoreLogs()
        {
            var sut = LoggingDetails.GetInstance();
            sut.IgnoreLogs = true;

            sut.ShouldNotNotifyOn(s => s.IgnoreLogs).When(s => s.ArchiveLogs = false);
            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.ArchiveLogs = true);

            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.ArchiveLogs = false);
        }

        [Test]
        public void Test_LoggingDetails_ChangingClearLogsShouldToggleIgnoreLogs()
        {
            var sut = LoggingDetails.GetInstance();
            sut.IgnoreLogs = true;

            sut.ShouldNotNotifyOn(s => s.IgnoreLogs).When(s => s.ClearLogs = false);
            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.ClearLogs = true);

            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.ClearLogs = false);
        }

        [Test]
        public void Test_LoggingDetails_Equals()
        {
            var path = Tests.Random<string>();
            var archive = Tests.Random<bool>();
            var clear = Tests.Random<bool>();

            var item1 = CreateTestObject(path, archive, clear);
            var item2 = CreateTestObject(path, archive, clear);
            var item3 = CreateTestObject(path, archive, clear);

            Tests.TestEquality(item1, item2, item3);

            // set them different by one item property.
            //==========================================
            item1.ArchivePath = path.Substring(path.Length/2);
            Assert.IsTrue(item2.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            item2.ArchivePath = item1.ArchivePath;
            item3.ArchivePath = item1.ArchivePath;
            item2.ArchiveLogs = !archive;
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsFalse(item1.Equals(item2));
            Assert.IsFalse(item3.Equals(item2));
            Tests.TestEquality(item1, item2, item3);

            item1.ArchiveLogs = item2.ArchiveLogs;
            item3.ArchiveLogs = item2.ArchiveLogs;
            item3.ClearLogs = !clear;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsFalse(item1.Equals(item3));
            Assert.IsFalse(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);

            // Set them equal.
            item1.ClearLogs = item3.ClearLogs;
            item2.ClearLogs = item3.ClearLogs;
            Assert.IsTrue(item1.Equals(item2));
            Assert.IsTrue(item1.Equals(item3));
            Assert.IsTrue(item2.Equals(item3));
            Tests.TestEquality(item1, item2, item3);
        }

        [Test]
        public void Test_LoggingDetails_GetHashCode()
        {
            var path = Tests.Random<string>();
            var archive = Tests.Random<bool>();
            var clear = Tests.Random<bool>();

            var item1 = CreateTestObject(path, archive, clear);
            var item2 = CreateTestObject(path, archive, clear);
            Assert.That(item1 == item2);

            // If two distinct objects compare as equal, their hashcodes must be equal.
            Tests.TestGetHashCode(item1, item2);
        }

        [Test]
        public void Test_LoggingDetails_IgnoreLogs()
        {
            var sut = LoggingDetails.GetInstance();

            // The following line sets ArchiveLogs and ClearLogs to false.
            sut.IgnoreLogs = true;

            // Shouldn't be able to uncheck ignore unless 
            // (at least one of) Archive or Clear are true.
            sut.ShouldNotNotifyOn(s => s.IgnoreLogs).When(s => s.IgnoreLogs = false);


            // Setting Both Archive and Clear to false should 
            // automatically toggle Ignore to true.
            sut.ArchiveLogs = true;
            sut.ClearLogs = true;
            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s =>
            {
                s.ArchiveLogs = false;
                s.ClearLogs = false;
            });
        }
        
        [Test]
        public void Test_LoggingDetails_IgnoreLogs_Updates()
        {
            var sut = LoggingDetails.GetInstance();
            sut.ArchiveLogs = false;
            sut.ClearLogs = false;
            sut.IgnoreLogs = true;

            sut.ShouldNotNotifyOn(s => s.IgnoreLogs).When(s => s.ArchiveLogs = false);
            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.ArchiveLogs = true);

            Assert.IsFalse(sut.IgnoreLogs);

            // Now it shouldn't notify again if we set Clearlogs to true
            sut.ShouldNotNotifyOn(s => s.IgnoreLogs).When(s => s.ClearLogs = true);

            // Now it should definitely renotify if we manually set it to true.
            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.IgnoreLogs = true);
        }

        [Test]
        public void Test_LoggingDetails_PropertyChanged_ArchiveLogs()
        {
            var sut = LoggingDetails.GetInstance();
            sut.IgnoreLogs = true;

            var currentValue = sut.ArchiveLogs;
            var newValue = !currentValue;

            sut.ShouldNotNotifyOn(s => s.ArchiveLogs).When(s => s.ArchiveLogs = currentValue);

            sut.ShouldNotifyOn(s => s.ArchiveLogs).When(s => s.ArchiveLogs = newValue);
            Assert.That(sut.ArchiveLogs == newValue);
        }

        [Test]
        public void Test_LoggingDetails_PropertyChanged_ArchivePath()
        {
            var sut = LoggingDetails.GetInstance();
            var currentValue = sut.ArchivePath;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.ArchivePath).When(s => s.ArchivePath = currentValue);

            sut.ShouldNotifyOn(s => s.ArchivePath).When(s => s.ArchivePath = newValue);
            Assert.That(sut.ArchivePath == newValue);
        }

        [Test]
        public void Test_LoggingDetails_PropertyChanged_ClearLogs()
        {
            var sut = LoggingDetails.GetInstance();
            var currentValue = sut.ClearLogs;
            var newValue = !currentValue;

            sut.ShouldNotNotifyOn(s => s.ClearLogs).When(s => s.ClearLogs = currentValue);

            sut.ShouldNotifyOn(s => s.ClearLogs).When(s => s.ClearLogs = newValue);
            Assert.That(sut.ClearLogs == newValue);
        }

        [Test]
        public void Test_LoggingDetails_PropertyChanged_Directory()
        {
            var sut = LoggingDetails.GetInstance();
            var currentValue = sut.Directory;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.Directory).When(s => s.Directory = currentValue);

            sut.ShouldNotifyOn(s => s.Directory).When(s => s.Directory = newValue);
            Assert.That(sut.Directory == newValue);
        }

        [Test]
        public void Test_LoggingDetails_PropertyChanged_DisplayName()
        {
            var sut = LoggingDetails.GetInstance();
            var currentValue = sut.DisplayName;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.DisplayName).When(s => s.DisplayName = currentValue);

            sut.ShouldNotifyOn(s => s.DisplayName).When(s => s.DisplayName = newValue);
            Assert.That(sut.DisplayName == newValue);
        }

        [Test]
        public void Test_LoggingDetails_PropertyChanged_FileName()
        {
            var sut = LoggingDetails.GetInstance();
            var currentValue = sut.FileName;
            var newValue = Path.GetRandomFileName();

            sut.ShouldNotNotifyOn(s => s.FileName).When(s => s.FileName = currentValue);

            sut.ShouldNotifyOn(s => s.FileName).When(s => s.FileName = newValue);
            Assert.That(sut.FileName == newValue);
        }

        [Test]
        public void Test_LoggingDetails_PropertyChanged_IgnoreLogs()
        {
            var sut = LoggingDetails.GetInstance();
            sut.IgnoreLogs = true;

            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.ArchiveLogs = true);
            Assert.IsFalse(sut.IgnoreLogs);

            sut.ShouldNotNotifyOn(s => s.IgnoreLogs).When(s => s.IgnoreLogs = false);
            Assert.IsFalse(sut.IgnoreLogs);

            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.IgnoreLogs = true);
            Assert.IsTrue(sut.IgnoreLogs);

            sut.ShouldNotifyOn(s => s.IgnoreLogs).When(s => s.ClearLogs = true);
            Assert.IsFalse(sut.IgnoreLogs);
        }
        
       
    }
}