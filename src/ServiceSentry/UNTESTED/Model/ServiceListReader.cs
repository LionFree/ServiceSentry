// -----------------------------------------------------------------------
//  <copyright file="ServiceListReader.cs" company="Accelrys, Inc.">
//      Copyright (c) 2014 Accelrys, Inc.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using ServiceSentry.Model.DataClasses;
using ServiceSentry.MonitorServiceReference;

#endregion

namespace ServiceSentry.Shit.Model
{
    public abstract class ServiceListReader
    {
        public static ServiceListReader GetInstance()
        {
            return new Implementation();
        }

        public abstract Service[] GetServices();

        public abstract bool IsInstalled(string serviceName);
        
        private sealed class Implementation : ServiceListReader
        {
            private readonly MonitorServiceClient _client;

            public Implementation()
            {
                _client = new MonitorServiceClient();
            }

            public override Service[] GetServices()
            {
                // Connect to correct machine.


                // Get Services
                var services = _client.GetServices();

                var output = new Service[services.Length];
                for (var i = 0; i < services.Length; i++)
                {
                    output[i] = Service.GetInstance(services[i].ServiceName, services[i].MachineName);
                }
                return output;
            }

            public override bool IsInstalled(string serviceName)
            {
                return _client.GetIsInstalled(serviceName);
            }

        }
    }
}