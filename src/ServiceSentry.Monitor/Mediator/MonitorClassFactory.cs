// -----------------------------------------------------------------------
//  <copyright file="MonitorClassFactory.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceModel;
using ServiceSentry.Common.Communication;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;
using ServiceSentry.Model.Communication;
using ServiceSentry.Monitor.Infrastructure;

#endregion

namespace ServiceSentry.Monitor.Mediator
{
    public abstract class MonitorClassFactory
    {
        internal abstract Logger GetLogger { get; }

        internal static MonitorClassFactory GetInstance(Logger logger)
        {
            return new MonitorClassFactoryImplementation(logger);
        }

        public abstract TrackingList GetState();
        public abstract MonitorEndpoint GetEndpoint(ConsoleHarness harness, ref TrackingList state);
        public abstract ConsoleHarness GetHarness();

        public abstract ServiceHost GetHost(WindowsServiceDescription description);

        public abstract ServerMediator GetController(SubscriptionPacket packet,
            ModelClassFactory factory, ConsoleHarness harness);

        private sealed class MonitorClassFactoryImplementation : MonitorClassFactory
        {
            private readonly Logger _logger;

            internal MonitorClassFactoryImplementation(Logger logger)
            {
                _logger = logger;
            }

            internal override Logger GetLogger
            {
                get { return _logger; }
            }

            public override TrackingList GetState()
            {
                return TrackingList.Default;
            }

            public override MonitorEndpoint GetEndpoint(ConsoleHarness harness, ref TrackingList state)
            {
                return MonitorEndpoint.GetInstance(_logger, harness, ref state);
            }

            public override ConsoleHarness GetHarness()
            {
                return ConsoleHarness.Default;
            }

            public override ServiceHost GetHost(WindowsServiceDescription description)
            {
                return HostBuilder.GetInstance(ServiceHostType.NetTcp).BuildHost(description);
            }

            public override ServerMediator GetController(SubscriptionPacket packet, ModelClassFactory factory,
                ConsoleHarness harness)
            {
                return ServerMediator.GetInstance(_logger, packet, factory, harness);
            }
        }
    }
}