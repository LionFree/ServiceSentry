// -----------------------------------------------------------------------
//  <copyright file="LoggingUtilities.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;

#endregion

namespace ServiceSentry.Extensibility.Logging
{
    public abstract class LoggingUtilities
    {
        public static LoggingUtilities GetInstance()
        {
            return new LoggingUtilitiesImplementation();
        }

        private sealed class LoggingUtilitiesImplementation : LoggingUtilities
        {
            public ObservableCollection<string> LogLevelNames
            {
                get
                {
                    var output = new ObservableCollection<string>();
                    return output;
                }
            }
        }
    }
}