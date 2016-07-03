// -----------------------------------------------------------------------
//  <copyright file="AssemblyTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using ServiceSentry.Annotations;
using ServiceSentry.Extensibility;
using ServiceSentry.Common.Testing;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Common.UnitTests.Assemblies
{

#pragma warning disable 1685

    [TestFixture]
    public class AssemblyTests
    {
        private readonly Mock<FileSystem> _fileSystem;
        private readonly Mock<TestAssembly> _assembly;


        public AssemblyTests()
        {
            _fileSystem = new Mock<FileSystem>();
            _assembly = new Mock<TestAssembly>();

            var codeBase = Environment.CurrentDirectory + "\\" + Path.GetRandomFileName();
            _assembly.Setup(a => a.CodeBase).Returns(codeBase);
        }

        [UsedImplicitly]
        private void Test_Attribute_with_Value(Attribute attribute, string attributeName, string expected)
        {
            // Arrange
            _assembly.Setup(a => a.GetCustomAttributes(It.Is<Type>(s => s == attribute.GetType()),
                                                       It.IsAny<bool>())).Returns(new object[] {attribute});

            // Act
            var sut = AssemblyWrapper.GetInstance(_assembly.Object, _fileSystem.Object);
            var actual = (string) sut.GetType().GetProperty(attributeName).GetValue(sut);

            // Assert
            Assert.IsTrue(actual == expected);
        }

        [UsedImplicitly]
        private void Test_Attribute_without_Value(Attribute attribute, string attributeName, string expected)
        {
            // Arrange
            _assembly.Setup(a => a.GetCustomAttributes(It.Is<Type>(s => s == attribute.GetType()),
                                                       It.IsAny<bool>())).Returns(new object[] {});

            // Act
            var sut = AssemblyWrapper.GetInstance(_assembly.Object, _fileSystem.Object);
            var actual = (string) sut.GetType().GetProperty(attributeName).GetValue(sut);

            // Assert
            Assert.IsTrue(actual == expected);
        }

        [Test, Explicit("Cannot unit test GetEntryAssembly.")]
        public void Test_Assembly_GetEntryAssembly()
        {
            Assert.Fail("Cannot unit test GetEntryAssembly.");
        }

        [Test]
        public void Test_AssemblyAttributes_AssemblyBuildDate()
        {
            // Arrange
            var converter = VersionConverter.Default;
            var version = new Version(Tests.Random<int>(), Tests.Random<int>(), Tests.Random<int>(),
                                      Tests.Random<int>());
            var name = new AssemblyName {Version = version};
            _assembly.Setup(m => m.GetName()).Returns(name);

            // Act
            var sut = AssemblyWrapper.GetInstance(_assembly.Object, _fileSystem.Object);
            var actual = sut.BuildDate;
            var expected = converter.ConvertVersionToDate(version).ToString(CultureInfo.InvariantCulture);

            // Assert
            Assert.IsTrue(actual == expected);
        }

        [Test]
        public void Test_AssemblyAttributes_AssemblyCompany()
        {
            var expected = Tests.Random<string>();
            Test_Attribute_with_Value(new AssemblyCompanyAttribute(expected), "AssemblyCompany", expected);
            Test_Attribute_without_Value(new AssemblyCompanyAttribute(null), "AssemblyCompany", "");
        }

        [Test]
        public void Test_AssemblyAttributes_AssemblyCopyright()
        {
            var expected = Tests.Random<string>();
            Test_Attribute_with_Value(new AssemblyCopyrightAttribute(expected), "AssemblyCopyright", expected);
            Test_Attribute_without_Value(new AssemblyCopyrightAttribute(null), "AssemblyCopyright", "");
        }

        [Test]
        public void Test_AssemblyAttributes_AssemblyDescription()
        {
            var expected = Tests.Random<string>();
            Test_Attribute_with_Value(new AssemblyDescriptionAttribute(expected), "AssemblyDescription", expected);
            Test_Attribute_without_Value(new AssemblyDescriptionAttribute(null), "AssemblyDescription", "");
        }

        [Test]
        public void Test_AssemblyAttributes_AssemblyProduct()
        {
            var expected = Tests.Random<string>();
            Test_Attribute_with_Value(new AssemblyProductAttribute(expected), "AssemblyProduct", expected);
            Test_Attribute_without_Value(new AssemblyProductAttribute(null), "AssemblyProduct", "");
        }

        [Test]
        public void Test_AssemblyAttributes_AssemblyTitle()
        {
            var expected = Tests.Random<string>();
            Test_Attribute_with_Value(new AssemblyTitleAttribute(expected), "AssemblyTitle", expected);

            expected = Path.GetFileNameWithoutExtension(_assembly.Object.CodeBase);
            Test_Attribute_without_Value(new AssemblyTitleAttribute(null), "AssemblyTitle", expected);
        }

        [Test]
        public void Test_AssemblyAttributes_AssemblyVersion()
        {
            // Arrange
            var expected = new Version(Tests.Random<int>(), Tests.Random<int>(), Tests.Random<int>(),
                                       Tests.Random<int>());
            var name = new AssemblyName {Version = expected};
            _assembly.Setup(m => m.GetName()).Returns(name);

            // Act
            var sut = AssemblyWrapper.GetInstance(_assembly.Object, _fileSystem.Object);
            var actual = sut.AssemblyVersion;

            // Assert
            Assert.IsTrue(actual == "Version " + expected);
        }

        [Test]
        public void Test_VersionConverter_ConvertVersionToDate()
        {
            var version = new Version(Tests.Random<int>(), Tests.Random<int>(), Tests.Random<int>(), Tests.Random<int>());

            var sut = VersionConverter.Default;
            var actual = sut.ConvertVersionToDate(version);
            Assert.That(actual != null);


            var staticV1 = new Version(1, 1, 1, 1);
            var staticD1 = new DateTime(2000, 1, 2, 0, 0, 2);
            actual = sut.ConvertVersionToDate(staticV1);

            var aStr = actual.ToString(CultureInfo.InvariantCulture);
            var eStr = staticD1.ToString(CultureInfo.InvariantCulture);

            Assert.That(actual != null);
            Assert.AreEqual(eStr, aStr);


            var staticV2 = new Version(5, 4, 3, 21);
            var staticD2 = new DateTime(2000, 1, 4, 0, 0, 42);
            actual = sut.ConvertVersionToDate(staticV2);

            aStr = actual.ToString(CultureInfo.InvariantCulture);
            eStr = staticD2.ToString(CultureInfo.InvariantCulture);

            Assert.That(actual != null);
            Assert.AreEqual(eStr, aStr);
        }

        [Test]
        public void Test_AssemblyAttributes_ProductVersion()
        {
            // Arrange
            var expected = Tests.Random<string>();

            _fileSystem.Setup(m => m.ProductVersion).Returns(expected);
            _assembly.Setup(m => m.Location).Returns(Tests.Random<string>());

            // Act
            var sut = AssemblyWrapper.GetInstance(_assembly.Object, _fileSystem.Object);
            var actual = sut.ProductVersion;

            // Assert
            Assert.IsTrue(actual == "Version " + expected);
        }

        [Test]
        public void Test_Assembly_BuildDate()
        {
            // Arrange
            var version = new Version(Tests.Random<int>(), Tests.Random<int>(), Tests.Random<int>(),
                                      Tests.Random<int>());
            var name = new AssemblyName {Version = version};
            var path = Tests.RandomFilePath();
            _assembly.Setup(m => m.GetName()).Returns(name);
            _assembly.Setup(m => m.Location).Returns(path);
            var converter = VersionConverter.Default;
            var sut = AssemblyWrapper.GetInstance(entryAssembly: _assembly.Object);

            // Act
            
            var actual = sut.BuildDate;
            var expected = converter.ConvertVersionToDate(version).ToString(CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}