// -----------------------------------------------------------------------
//  <copyright file="IMonitorService.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.ServiceModel;
using ServiceSentry.Model.Enumerations;

#endregion

namespace ServiceSentry.Model.Communication
{
    [ServiceContract]
    public interface IMonitorService
    {
        /// <summary>
        ///     Gets a value indicating whether the service can be stopped after it has started.
        /// </summary>
        /// <returns>
        ///     true if the service can be stopped; otherwise, false.
        /// </returns>
        /// <param name="serviceName">The name of the service to check.</param>
        [OperationContract]
        bool GetCanStop(string serviceName);

        /// <summary>
        ///     Gets or sets a friendly name for the service.
        /// </summary>
        /// <returns>
        ///     The friendly name of the service, which can be used to identify the service.
        /// </returns>
        /// <param name="serviceName">The name of the service to check.</param>
        [OperationContract]
        string GetDisplayName(string serviceName);

        /// <summary>
        ///     Gets a value indicating whether the service is installed or not.
        /// </summary>
        /// <returns>
        ///     true if the service is installed; otherwise, false.
        /// </returns>
        /// <param name="serviceName">The name of the service to check.</param>
        [OperationContract]
        bool GetIsInstalled(string serviceName);

        /// <summary>
        ///     Gets the status of the service that is referenced by this instance.
        /// </summary>
        /// <returns>
        ///     One of the <see cref="T:ServiceSentry.Model.Enumerations.ServiceState" /> values that indicates whether the service is running, stopped, or paused, or whether a start, stop, pause, or continue command is pending.
        /// </returns>
        /// <param name="serviceName">The name of the service to check.</param>
        [OperationContract]
        PollResult GetStatus(string serviceName);

        /// <summary>
        ///     Starts the service, passing no arguments.
        /// </summary>
        /// <param name="serviceData">The details of the service to wait for.</param>
        [OperationContract]
        void Start(SubscriptionPacket serviceData);

        /// <summary>
        ///     Stops the service, passing no arguments.
        /// </summary>
        /// <param name="serviceData">The details of the service to wait for.</param>
        [OperationContract]
        void Stop(SubscriptionPacket serviceData);

        /// <summary>
        ///     Refreshes property values by resetting the properties to their current values.
        /// </summary>
        /// <param name="serviceData">The details of the service to wait for.</param>
        [OperationContract]
        void Refresh(SubscriptionPacket serviceData);

        /// <summary>
        ///     Waits for the service to reach the specified status or for the specified time-out to expire.
        /// </summary>
        /// <param name="serviceData">The details of the service to wait for.</param>
        /// <param name="desiredStatus">The status to wait for. </param>
        [OperationContract]
        void WaitForStatus(SubscriptionPacket serviceData, ServiceState desiredStatus);

        /// <summary>
        ///     Retrieves all the services on the local computer, except for the device driver services.
        /// </summary>
        /// <returns>
        ///     An array of type <see cref="T:ServiceSentry.Model.Communication.SubscriptionPacket" /> in
        ///     which each element is associated with a service on the local computer.
        /// </returns>
        [OperationContract]
        SubscriptionPacket[] GetServices();

        /// <summary>
        ///     Instructs the ServiceMonitor to begin listening to the status of the designated service.
        /// </summary>
        /// ///
        /// <param name="serviceData">The details of the service to listen to.</param>
        [OperationContract]
        void Subscribe(SubscriptionPacket serviceData);

        /// <summary>
        ///     Instructs the ServiceMonitor to stop listening to the status of the designated service.
        /// </summary>
        /// <param name="serviceName">The name of the service to stop listening to.</param>
        [OperationContract]
        void Unsubscribe(string serviceName);

        /// <summary>
        ///     Instructs the ServiceMonitor to stop listening to the status of the designated service.
        /// </summary>
        /// ///
        /// <param name="serviceData">The details of the service to stop listening to.</param>
        [OperationContract]
        void UpdateSubscription(SubscriptionPacket serviceData);
    }
}