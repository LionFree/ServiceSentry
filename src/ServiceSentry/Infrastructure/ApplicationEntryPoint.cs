using System;

namespace ServiceSentry.Client.Infrastructure
{
    internal static class ApplicationEntryPoint
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Engine.CreateObjectGraph().Start(args);
        }
    }
}