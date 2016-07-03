// -----------------------------------------------------------------------
//  <copyright file="ServiceListBehaviorTests.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Moq;
using NUnit.Framework;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Enumerations;
using ServiceSentry.Model.ExternalFiles;
using ServiceSentry.UnitTests.Common;

#endregion

namespace ServiceSentry.Model.UnitTests.Model.Behavior
{
    [TestFixture]
    internal class ServiceListBehaviorTests
    {
        [SetUp]
        public void SetUp()
        {
            _completed = false;
            _archived = false;
        }

        private readonly SyncWorker _worker;
        private readonly Mock<ServiceTogglingBehavior> _behavior;
        private readonly Mock<Logger> _logger;
        private readonly Mock<LogArchiver> _archiver;
        private bool _completed;
        private bool _archived;
        private const int NumServices = 3;


        private void Start_Callback()
        {
            _behavior.Setup(m => m.Start(It.IsAny<Service>()))
                     .Callback(Start_Callback)
                     .Returns(ServiceState.Running);
        }

        private void Stop_Callback()
        {
            _behavior.Setup(m => m.Stop(It.IsAny<Service>()))
                     .Callback(Stop_Callback)
                     .Returns(ServiceState.Stopped);
        }

        public ServiceListBehaviorTests()
        {
            _worker = SyncWorker.Default;
            _behavior = new Mock<ServiceTogglingBehavior>();
            _logger = new Mock<Logger>();

            Start_Callback();
            Stop_Callback();

            _archiver = new Mock<LogArchiver>();
            _archiver.Setup(m => m.ArchiveAndClearLogs(
                It.IsAny<List<FileInfoWrapper>>(),
                It.IsAny<LoggingDetails>())).Callback(() => { _archived = true; });
        }


        private Mock<ServiceList> Create_Mock_ServiceList(
            ServiceState start,
            ServiceState finish)
        {
            var deets1 = ServiceDetails.GetInstance();
            deets1.StartOrder = 1;
            deets1.StopOrder = 1;

            var deets2 = ServiceDetails.GetInstance();
            deets1.StartOrder = 2;
            deets1.StopOrder = 2;

            var deets3 = ServiceDetails.GetInstance();
            deets1.StartOrder = 3;
            deets1.StopOrder = 3;

            var svc1 = new Mock<Service>();
            var svc2 = new Mock<Service>();
            var svc3 = new Mock<Service>();

            svc1.Setup(m => m.Status).Returns(start);
            svc2.Setup(m => m.Status).Returns(start);
            svc3.Setup(m => m.Status).Returns(start);
            svc1.Setup(m => m.Details).Returns(deets1);
            svc2.Setup(m => m.Details).Returns(deets2);
            svc3.Setup(m => m.Details).Returns(deets3);

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            svc1.Setup(m => m.Guid).Returns(guid1);
            svc2.Setup(m => m.Guid).Returns(guid2);
            svc3.Setup(m => m.Guid).Returns(guid3);


            var logFile1 = new Mock<ExternalFile>();
            var logFile2 = new Mock<ExternalFile>();
            var logFile3 = new Mock<ExternalFile>();
            var path1 = Tests.RandomFilePath();
            var path2 = Tests.RandomFilePath();
            var path3 = Tests.RandomFilePath();
            logFile1.Setup(m => m.ParsedName).Returns(path1);
            logFile2.Setup(m => m.ParsedName).Returns(path2);
            logFile3.Setup(m => m.ParsedName).Returns(path3);
            var logFiles = new ObservableCollection<ExternalFile> {logFile1.Object, logFile2.Object, logFile3.Object};
            svc1.Setup(m => m.LogFiles).Returns(logFiles);
            svc2.Setup(m => m.LogFiles).Returns(logFiles);
            svc3.Setup(m => m.LogFiles).Returns(logFiles);


            _behavior.Setup(m => m.Start(svc1.Object)).Callback(() => svc1.Setup(m => m.Status).Returns(finish));
            _behavior.Setup(m => m.Stop(svc1.Object)).Callback(() => svc1.Setup(m => m.Status).Returns(finish));
            _behavior.Setup(m => m.Refresh(svc1.Object)).Callback(() => svc1.Setup(m => m.Status).Returns(finish));

            _behavior.Setup(m => m.Start(svc2.Object)).Callback(() => svc2.Setup(m => m.Status).Returns(finish));
            _behavior.Setup(m => m.Stop(svc2.Object)).Callback(() => svc2.Setup(m => m.Status).Returns(finish));
            _behavior.Setup(m => m.Refresh(svc2.Object)).Callback(() => svc2.Setup(m => m.Status).Returns(finish));

            _behavior.Setup(m => m.Start(svc3.Object)).Callback(() => svc3.Setup(m => m.Status).Returns(finish));
            _behavior.Setup(m => m.Stop(svc3.Object)).Callback(() => svc3.Setup(m => m.Status).Returns(finish));
            _behavior.Setup(m => m.Refresh(svc3.Object)).Callback(() => svc3.Setup(m => m.Status).Returns(finish));

            var items = new ObservableCollection<Service> {svc1.Object, svc2.Object, svc3.Object};
            var details = LoggingDetails.GetInstance();
            details.ArchiveLogs = true;

            var svcList = new Mock<ServiceList>();
            svcList.Setup(m => m.Items).Returns(items);
            svcList.Setup(m => m.LogDetails).Returns(details);

            return svcList;
        }

        [Test]
        public void Test_ServiceListBehavior_Restart()
        {
            // Arrange
            const ServiceState current = ServiceState.Running;
            const ServiceState expected = ServiceState.Running;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var service = svcList.Object.Items[Tests.Random<int>(0, NumServices - 1)];
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.Restart(service, svcList.Object.LogDetails);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(_archived);
        }

        [Test]
        public void Test_ServiceListBehavior_RestartAll()
        {
            // Arrange
            const ServiceState current = ServiceState.Running;
            const ServiceState expected = ServiceState.Running;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.RestartAll(svcList.Object);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(_archived);
        }

        [Test]
        public void Test_ServiceListBehavior_RestartGroup()
        {
            // Arrange
            const ServiceState current = ServiceState.Running;
            const ServiceState expected = ServiceState.Running;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.RestartGroup(svcList.Object.Items, svcList.Object.LogDetails);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceListBehavior_Start()
        {
            // Arrange
            const ServiceState current = ServiceState.Stopped;
            const ServiceState expected = ServiceState.Running;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var service = svcList.Object.Items[Tests.Random<int>(0, NumServices - 1)];
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.Start(service, svcList.Object.LogDetails);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceListBehavior_StartAll()
        {
            // Arrange
            const ServiceState current = ServiceState.Stopped;
            const ServiceState expected = ServiceState.Running;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.StartAll(svcList.Object);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceListBehavior_StartGroup()
        {
            // Arrange
            const ServiceState current = ServiceState.Stopped;
            const ServiceState expected = ServiceState.Running;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.StartGroup(svcList.Object.Items, svcList.Object.LogDetails);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceListBehavior_Start_Running()
        {
            // Arrange
            const ServiceState current = ServiceState.Running;
            const ServiceState expected = ServiceState.Running;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var service = svcList.Object.Items[Tests.Random<int>(0, NumServices - 1)];
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.Start(service, svcList.Object.LogDetails);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_ServiceListBehavior_Stop()
        {
            // Arrange
            const ServiceState current = ServiceState.Running;
            const ServiceState expected = ServiceState.Stopped;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var service = svcList.Object.Items[Tests.Random<int>(0, NumServices - 1)];
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.Stop(service, svcList.Object.LogDetails);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(_archived);
        }

        [Test]
        public void Test_ServiceListBehavior_StopAll()
        {
            // Arrange
            const ServiceState current = ServiceState.Running;
            const ServiceState expected = ServiceState.Stopped;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.StopAll(svcList.Object);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(_archived);
        }

        [Test]
        public void Test_ServiceListBehavior_StopGroup()
        {
            // Arrange
            const ServiceState current = ServiceState.Running;
            const ServiceState expected = ServiceState.Stopped;
            object actual = current;
            var svcList = Create_Mock_ServiceList(current, expected);
            var sut = ServiceListBehavior.GetInstance(_logger.Object, _behavior.Object, _worker, _archiver.Object);
            sut.WorkerCompleted += (s, e) =>
                {
                    _completed = true;
                    var args = e as RunWorkerCompletedEventArgs;
                    if (args == null) Assert.Fail("No arguments passed to onComplete.");
                    actual = args.Result;
                };

            // Act
            sut.StopGroup(svcList.Object.Items, svcList.Object.LogDetails);

            // Assert
            Assert.IsTrue(_completed);
            Assert.AreEqual(expected, actual);
        }
    }
}