// -----------------------------------------------------------------------
//  <copyright file="TrackingListTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.DataClasses
{
    [TestFixture]
    internal class TrackingListTests
    {
        private readonly string _key;
        private readonly TrackingObject _value;
        private readonly KeyValuePair<string, TrackingObject> _kvp;

        public TrackingListTests()
        {
            _key = Tests.Random<string>();
            _value = new TrackingObject {ServiceName = _key};
            _kvp = new KeyValuePair<string, TrackingObject>(_key, _value);
        }

        [Test]
        public void Test_Add_KeyAndValue()
        {
            // Arrange
            var sut = TrackingList.Default;

            // Act
            Assert.DoesNotThrow(() => sut.Add(_key, _value));

            // Assert
            Assert.IsNotNull(sut[_key]);
            Assert.AreEqual(sut[_key], _value);
        }

        [Test]
        public void Test_Add_KeyValuePair()
        {
            // Arrange
            var sut = TrackingList.Default;

            // Act
            Assert.DoesNotThrow(() => sut.Add(_kvp));

            // Assert
            Assert.IsNotNull(sut[_key]);
            Assert.AreEqual(sut[_key], _value);
        }

        [Test]
        public void Test_Clear()
        {
            // Arrange
            var sut = TrackingList.Default;
            sut.Add(_kvp);

            // Act
            Assert.DoesNotThrow(sut.Clear);

            // Assert
            Assert.AreEqual(0, sut.Count);
            Assert.Throws<KeyNotFoundException>(
                () =>
                    {
                        var p = sut[_key];
                        Trace.WriteLine(p.ServiceName);
                    });
        }

        [Test]
        public void Test_Count()
        {
            // Arrange
            var expected = Tests.Random<int>(25);
            var sut = TrackingList.Default;
            for (var i = 0; i < expected; i++)
            {
                var name = Tests.Random<string>();
                var value = new TrackingObject { ServiceName = name };
                sut.Add(name, value);
            }

            // Act
            var actual = sut.Count;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Remove_Key()
        {
            // Arrange
            var sut = TrackingList.Default;
            sut.Add(_kvp);

            // Act
            Assert.DoesNotThrow(() => sut.Remove(_key));

            // Assert
            Assert.Throws<KeyNotFoundException>(
                () =>
                    {
                        var p = sut[_key];
                        Trace.WriteLine(p.ServiceName);
                    });
        }

        [Test]
        public void Test_Remove_KeyValuePair()
        {
            // Arrange
            var sut = TrackingList.Default;
            sut.Add(_kvp);

            // Act
            Assert.DoesNotThrow(() => sut.Remove(_kvp));

            // Assert
            Assert.Throws<KeyNotFoundException>(
                () =>
                    {
                        var p = sut[_key];
                        Trace.WriteLine(p.ServiceName);
                    });
        }
    }
}