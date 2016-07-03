// -----------------------------------------------------------------------
//  <copyright file="ClientBuilder.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Diagnostics;
//using System.Diagnostics.Contracts;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ServiceSentry.Common.Communication;

#endregion

namespace ServiceSentry.Model.Client
{
    /// <summary>
    ///     Builds a <c>ServiceClient&lt;T&gt;</c> of a specific type, given the
    ///     client type, the server address, and the endpoint.
    /// </summary>
    internal abstract class ClientBuilder
    {
        /// <summary>
        ///     Builds a <see cref="ServiceClient{T}" /> of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type (interface) of client to build.</typeparam>
        /// <param name="server">The machine to talk to.</param>
        /// <param name="endpointSuffix">The endpoint on the target machine.</param>
        /// <returns>
        ///     A <see cref="ServiceClient{T}" /> of type <typeparamref name="T" />.
        /// </returns>
        internal abstract ServiceClient<T> GetClient<T>(string server, string endpointSuffix) where T : class;

        internal static ClientBuilder GetInstance()
        {
            return new ClientBuilderImplementation(ClientConfigBuilder.GetInstance(ServiceHostType.NetTcp));
        }

        private class ClientBuilderImplementation : ClientBuilder
        {
            private readonly ClientConfigBuilder _configBuilder;

            internal ClientBuilderImplementation(ClientConfigBuilder configBuilder)
            {
                _configBuilder = configBuilder;
            }

            internal override ServiceClient<T> GetClient<T>(string server, string endpointSuffix)
            {
                var machine = (server == "." || server.ToLower() == "localhost")
                                  ? Dns.GetHostEntry("LocalHost").HostName
                                  : server;

                var endpoint = _configBuilder.GetEndpoint(machine, endpointSuffix, -1);

                var binding = _configBuilder.GetBinding();

                return
                    ServiceClient<T>.GetInstance(ChannelExposingClient<T>.GetInstance(binding, endpoint));
            }
        }
    }

    public abstract class ServiceClient<TService> : IDisposable where TService : class
    {
        public abstract bool IsAvailable { get; }
        public abstract void Open();
        public abstract bool IsLocal { get; }
        public abstract TService Service { get; }
        public abstract void Dispose();

        /// <summary>
        ///     Executes an <see cref="T:System.Action`1" /> with a single parameter.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type" /> of the parameter being passed to the method.
        /// </typeparam>
        /// <param name="action">
        ///     The <see cref="T:System.Action`1" /> (method) to execute.
        /// </param>
        /// <param name="parameter">The parameter to pass to the method.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public abstract bool Execute<T>(Action<T> action, T parameter);

        /// <summary>
        ///     Executes an <see cref="T:System.Action`1" /> with a single parameter.  If that method throws
        ///     an <see cref="Exception" />, executes a fallback <see cref="T:System.Action`2" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type" /> of the parameter being passed to the method.
        /// </typeparam>
        /// <param name="action">
        ///     The <see cref="T:System.Action`1" /> (method) to execute.
        /// </param>
        /// <param name="failback">
        ///     The <see cref="T:System.Action`2" /> (method) to execute if
        ///     <paramref name="action" /> throws an <see cref="Exception" />.
        /// </param>
        /// <param name="parameter">The parameter to pass to the method.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public abstract bool Execute<T>(Action<T> action, Action<T, Exception> failback, T parameter);

        /// <summary>
        ///     Executes an <see cref="T:System.Action`2" /> with two parameters.
        /// </summary>
        /// <typeparam name="T1">
        ///     The <see cref="Type" /> of the first parameter being passed to the method.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The <see cref="Type" /> of the second parameter being passed to the method.
        /// </typeparam>
        /// <param name="action">
        ///     The <see cref="T:System.Action`2" /> (method) to execute.
        /// </param>
        /// <param name="parameter1">The first parameter to pass to the method.</param>
        /// <param name="parameter2">The second parameter to pass to the method.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public abstract bool Execute<T1, T2>(Action<T1, T2> action, T1 parameter1, T2 parameter2);

        /// <summary>
        ///     Executes an <see cref="T:System.Action`2" /> with two parameter.  If that method throws
        ///     an <see cref="Exception" />, executes a fallback <see cref="T:System.Action`3" />.
        /// </summary>
        /// <typeparam name="T1">
        ///     The <see cref="Type" /> of the first parameter being passed to the method.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The <see cref="Type" /> of the second parameter being passed to the method.
        /// </typeparam>
        /// <param name="action">
        ///     The <see cref="T:System.Action`2" /> (method) to execute.
        /// </param>
        /// <param name="failback">
        ///     The <see cref="T:System.Action`3" /> (method) to execute if
        ///     <paramref name="action" /> throws an <see cref="Exception" />.
        /// </param>
        /// <param name="parameter1">The first parameter to pass to the method.</param>
        /// <param name="parameter2">The second parameter to pass to the method.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public abstract bool Execute<T1, T2>(Action<T1, T2> action, Action<T1, T2, Exception> failback, T1 parameter1,
                                             T2 parameter2);

        internal static ServiceClient<TService> GetInstance(ChannelExposingClient<TService> toWrap)
        {
            //Contract.Requires(toWrap != null);
            return new ServiceClientImplementation(toWrap);
        }


        private class ServiceClientImplementation : ServiceClient<TService>
        {
            private readonly bool _isLocal;
            private readonly ChannelExposingClient<TService> _wrapped;

            public ServiceClientImplementation(ChannelExposingClient<TService> wrapped)
            {
                //Contract.Requires(wrapped != null);
                _wrapped = wrapped;
                _isLocal = (wrapped.Endpoint.ListenUri.Host == Dns.GetHostEntry("LocalHost").HostName);
            }

            public override void Open()
            {
                try
                {
                    _wrapped.Open();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }

            public override bool IsLocal
            {
                get { return _isLocal; }
            }

            public override bool IsAvailable
            {
                get
                {   
                    return _wrapped.State == CommunicationState.Opened;
                }
            }

            public override TService Service
            {
                get { return _wrapped.Service; }
            }

            public override bool Execute<T>(Action<T> action, T parameter)
            {
                return Execute(action, null, parameter);
            }

            public override bool Execute<T1, T2>(Action<T1, T2> action, T1 parameter1, T2 parameter2)
            {
                return Execute(action, null, parameter1, parameter2);
            }

            public override bool Execute<T1, T2>(Action<T1, T2> action, Action<T1, T2, Exception> failback,
                                                 T1 parameter1, T2 parameter2)
            {
                try
                {
                    action.Invoke(parameter1, parameter2);
                    return true;
                }
                catch (Exception ex)
                {
                    if (failback == null) return false;

                    return FailBack(failback, parameter1, parameter2, ex);
                }
            }

            public override bool Execute<T>(Action<T> action, Action<T, Exception> failback, T parameter)
            {
                try
                {
                    action.Invoke(parameter);
                    return true;
                }
                catch (Exception ex)
                {
                    if (failback == null) return false;

                    return FailBack(failback, parameter, ex);
                }
            }

            private static bool FailBack<T>(Action<T, Exception> failback, T parameter, Exception ex)
            {
                //Contract.Requires(failback != null);
                try
                {
                    failback.Invoke(parameter, ex);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            private static bool FailBack<T1, T2>(Action<T1, T2, Exception> failback, T1 parameter1, T2 parameter2,
                                                 Exception ex)
            {
                //Contract.Requires(failback != null);
                try
                {
                    failback.Invoke(parameter1, parameter2, ex);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public override void Dispose()
            {
                if (_wrapped.State != CommunicationState.Closed &&
                    _wrapped.State != CommunicationState.Closing)
                {
                    if (_wrapped.State != CommunicationState.Faulted)
                    {
                        _wrapped.Close();
                    }
                    else
                    {
                        _wrapped.Abort();
                    }
                }
                _wrapped.Dispose();
            }
        }
    }

    internal abstract class ChannelExposingClient<TChannel> : ClientBase<TChannel> where TChannel : class
    {
        protected ChannelExposingClient(Binding binding, string endpoint) : base(binding, new EndpointAddress(endpoint))
        {
        }

        public abstract TChannel Service { get; }

        internal static ChannelExposingClient<TChannel> GetInstance(Binding binding, string endpoint)
        {
            return new ChannelExposingClientImplementation(binding, endpoint);
        }

        public abstract void Dispose();

        private class ChannelExposingClientImplementation : ChannelExposingClient<TChannel>
        {
            public ChannelExposingClientImplementation(Binding binding, string endpoint)
                : base(binding, endpoint)
            {
            }

            public override TChannel Service
            {
                get { return Channel; }
            }

            public override void Dispose()
            {
                try
                {
                    if (State != CommunicationState.Opened)
                    {
                        Abort();
                    }
                    else
                    {
                        Close();
                    }
                }
                    // ReSharper disable EmptyGeneralCatchClause
                catch
                {
                }
                // ReSharper restore EmptyGeneralCatchClause
            }
        }
    }
}