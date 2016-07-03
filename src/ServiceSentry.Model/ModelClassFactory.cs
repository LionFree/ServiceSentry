// -----------------------------------------------------------------------
//  <copyright file="ModelClassFactory.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
//using System.Diagnostics.Contracts;
using ServiceSentry.Common.Email;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model.Client;
using ServiceSentry.Model.Communication;
using ServiceSentry.Model.Email;
using ServiceSentry.Model.Server;
using ServiceSentry.Model.Services;

#endregion

namespace ServiceSentry.Model
{
    public abstract class ModelClassFactory
    {
        public static ModelClassFactory GetInstance(Logger logger)
        {
            return new ModelClassFactoryImplementation(logger); 
        }
        
        public abstract ServiceWrapper GetLocalServiceController(string serviceName);
        public abstract LocalServiceFinder GetLocalServiceFinder();
        public abstract ClientMediator GetClientMediator(ClientList clients, SubscriptionPacket packet);
        public abstract Responder GetResponder();
        public abstract Emailer GetEmailer(SMTPInfo smtpInfo);

        private sealed class ModelClassFactoryImplementation : ModelClassFactory
        {
            private readonly Logger _logger;

            internal ModelClassFactoryImplementation(Logger logger)
            {
                //Contract.Requires(logger != null);
               _logger = logger;
           }

            public override ServiceWrapper GetLocalServiceController(string serviceName)
            {
                return ServiceWrapper.GetInstance(serviceName);
            }

            public override LocalServiceFinder GetLocalServiceFinder()
            {
                return LocalServiceFinder.Default;
            }

            public override ClientMediator GetClientMediator(ClientList clients, SubscriptionPacket packet)
            {
                var output = ClientMediator.GetInstance(_logger, clients, packet, this);
                if (output != null) return output;

                _logger.Error(Strings.Error_CouldNotGenerateClientMediator, packet.MachineName, packet.ServiceName);
                throw new Exception(string.Format(Strings.Error_CouldNotGenerateClientMediator, packet.MachineName,
                    packet.ServiceName));
            }

            public override Responder GetResponder()
            {
                return Responder.GetInstance(_logger);
            }

            public override Emailer GetEmailer(SMTPInfo info)
            {
                return Emailer.GetInstance(info);
            }
  
        }
    }
}