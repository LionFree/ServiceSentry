// -----------------------------------------------------------------------
//  <copyright file="ZipperTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Ionic.Zip;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;

#endregion

namespace ServiceSentry.Common.UnitTests.File_System
{
    [TestFixture]
    internal class ZipperTests
    {
        private const string UnpackDirectory = "C:\\Files\\Debug\\ExtractedArchive";
        private readonly string _zipFileName = Environment.CurrentDirectory + "\\" + "TestZipFile.zip";
        private DirectoryInfo _directory;
        private Zipper _sut;
        private FileInfoWrapper _zipFile;

        private FileInfoWrapper[] GetFilesInCurrentDirectory()
        {
            return (FileSystem.GetUnloggedInstance()).GetFiles();
        }

        [SetUp]
        public void SetUp()
        {
            _sut = Zipper.GetInstance(Logger.Null);
            _zipFile = FileInfoWrapper.GetInstance(_zipFileName);
            if (_zipFile.Exists) _zipFile.Delete();

            _directory = new DirectoryInfo(UnpackDirectory);
            if (_directory.Exists) _directory.Delete(true);

            try
            {
                _directory.Create();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _zipFile.Refresh();
            if (_zipFile.Exists) _zipFile.Delete();

            _directory = new DirectoryInfo(UnpackDirectory);
            if (_directory.Exists) _directory.Delete(true);
        }

        private void UnzipFile(string path)
        {
            using (var zipFile = ZipFile.Read(path))
            {
                foreach (var item in zipFile)
                {
                    item.Extract(UnpackDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        private void ValidateZipContents(string zipFilePath, IEnumerable<FileInfoWrapper> files)
        {
            UnzipFile(zipFilePath);
            var filesExist = true;
            var directory = (new DirectoryInfo(UnpackDirectory)).GetFiles();

            foreach (var expectedItem in files)
            {
                var exists = false;
                foreach (var realItem in directory)
                {
                    if (expectedItem.Name == realItem.Name) exists = true;
                }
                filesExist = filesExist && exists;
            }

            Assert.IsTrue(filesExist);
        }


        [Test]
        public void Test_Zipper_AddFiles_nullPath()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.ZipFiles(null, null));
        }

        [Test]
        public void Test_Zipper_AddFiles_nullFiles()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.ZipFiles(_zipFileName, null));
        }
        
        [Test, Explicit("Touches the File System.")]
        public void Test_Zipper_AddFiles()
        {
            // Arrange
            var files = GetFilesInCurrentDirectory();

            // Act
            _sut.ZipFiles(_zipFileName, files);

            // Assert
            _zipFile.Refresh();
            Assert.IsTrue(_zipFile.Exists);
            ValidateZipContents(_zipFileName, files);
        }
    }
}