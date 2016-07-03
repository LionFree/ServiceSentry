using System;

namespace ServiceSentry.Common.Communication
{
    public sealed class WindowsServiceDescription
    {
        public Type Contract;
        public string EndpointSuffix;
        public int Port = -1;
        public object ServiceObject;
        public Type ServiceType;
    }
}