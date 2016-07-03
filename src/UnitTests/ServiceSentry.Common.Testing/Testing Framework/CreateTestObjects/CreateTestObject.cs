// -----------------------------------------------------------------------
//  <copyright file="CreateTestObject.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.ObjectModel;
using System.Net.Mail;
using Moq;
using ServiceSentry.Common.Email;
using ServiceSentry.Extensibility;
using ServiceSentry.Extensibility.Logging;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static EmailInfo CreateTestObject(ObservableCollection<string> cc, string from, bool isBodyHtml,
                                                 MailPriority priority, string replyTo, string sender,
                                                 ObservableCollection<string> to)
        {
            var output = EmailInfo.Default;
            {
                output.CC = cc;
                output.From = from;
                output.IsBodyHtml = isBodyHtml;
                output.Priority = priority;
                output.ReplyTo = replyTo;
                output.SenderAddress = sender;
                output.To = to;
            }

            return output;
        }


        public static Service CreateTestObject(string serviceName, string commonName = "")
        {
            var output = Service.GetInstance(serviceName, ".", Logger.Null);
            output.CommonName = commonName;
            return output;
        }


        public static ConfigFile CreateTestFile()
        {
            var fsb = new Mock<FileSystem>();
            var logger = new Mock<Logger>();

            var serviceFile = ConfigFile.Default;

            var wuauService = Service.GetInstance("wuauserv", ".", Logger.Null);
            var wuauLog = ExternalFile.GetInstance(logger.Object, "C:\\Windows\\WindowsUpdate.log", "Windows Update Log");
            wuauService.LogFiles.Add(wuauLog);

            var wuauConfig = ExternalFile.GetInstance(logger.Object,"C:\\Windows\\System.ini","system.ini" );
            wuauService.ConfigFiles.Add(wuauConfig);

            serviceFile.Services.Items.Add(wuauService);

            var bonusLog = ExternalFile.GetInstance(logger.Object, "C:\\Windows\\System32\\GfxUI.exe.config",
                                                    "Graphics UI config file");
            serviceFile.LogList.Items.Add(bonusLog);

            return serviceFile;
        }
    }
}