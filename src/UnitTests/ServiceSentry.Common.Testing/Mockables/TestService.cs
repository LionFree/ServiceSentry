// -----------------------------------------------------------------------
//  <copyright file="TestService.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.ServiceModel;
using Moq;
using ServiceSentry.Common.ServiceFramework;
using ServiceSentry.Extensibility;

#endregion

namespace ServiceSentry.Common.Testing
{
    public abstract class TestWindowsService : WindowsService, ITestService
    {
        private const string ExpectedName = "Random Service Name";
        public abstract bool Operation();

        public static TestWindowsService GetInstance()
        {
            return new Implementation();
        }

        public event EventHandler Start;
        public event EventHandler Stop;
        public event EventHandler Shutdown;
        public event EventHandler Pause;
        public event EventHandler Continue;


        [WindowsService(ExpectedName, CanStop = true, CanPauseAndContinue = true, CanShutdown = true)]
        private sealed class Implementation : TestWindowsService
        {
            private readonly ConsoleHarness _harness;
            private readonly Mock<AssemblyWrapper> _assembly;
            private readonly Mock<ConsoleWrapper> _console;

            internal Implementation()
            {
                _assembly = new Mock<AssemblyWrapper>();
                _console=new Mock<ConsoleWrapper>();
                _harness = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);
            }

            public override ConsoleHarness Harness
            {
                get { return _harness; }
            }

            public override string ServiceName
            {
                get { throw new NotImplementedException(); }
            }

            public override string Endpoint
            {
                get { throw new NotImplementedException(); }
            }

            public override void OnStart(string[] args)
            {
                var handler = Start;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public override void OnStop()
            {
                var handler = Stop;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public override void OnPause()
            {
                var handler = Pause;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public override void OnContinue()
            {
                var handler = Continue;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public override void OnShutdown()
            {
                var handler = Shutdown;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public override void OnCustomCommand(int command)
            {
                throw new NotImplementedException();
            }

            public override bool Operation()
            {
                throw new NotImplementedException();
            }
        }
    }

    public abstract class UnmarkedWindowsService : WindowsService
    {
        public static UnmarkedWindowsService GetInstance()
        {
            return new Implementation();
        }

        private sealed class Implementation : UnmarkedWindowsService
        {
            private readonly Mock<ConsoleWrapper> _console;
            private readonly ConsoleHarness _harness;
            private readonly Mock<AssemblyWrapper> _assembly;

            internal Implementation()
            {
                _console=new Mock<ConsoleWrapper>();
                _assembly=new Mock<AssemblyWrapper>();
                _harness = ConsoleHarness.GetInstance(_assembly.Object, _console.Object);
            }

            public override ConsoleHarness Harness
            {
                get { return _harness; }
            }

            public override string ServiceName
            {
                get { throw new NotImplementedException(); }
            }

            public override string Endpoint
            {
                get { throw new NotImplementedException(); }
            }

            public override void OnStart(string[] args)
            {
                throw new NotImplementedException();
            }

            public override void OnStop()
            {
                throw new NotImplementedException();
            }

            public override void OnCustomCommand(int command)
            {
                throw new NotImplementedException();
            }
        }
    }

    public abstract class NotAWindowsService
    {
        public static NotAWindowsService GetInstance()
        {
            return new Implementation();
        }

        private sealed class Implementation : NotAWindowsService
        {
        }
    }


    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        bool Operation();
    }
}