using System.Windows;
using ServiceSentry.Client.UNTESTED.ViewModels;

namespace ServiceSentry.Client.Views.Resources
{
  public static class ResourceKeys
  {
      #region Application Icons

      public static readonly ComponentResourceKey ApplicationIconKey =
              new ComponentResourceKey(typeof(ResourceKeys), resourceId: "ApplicationIcon");

      public static readonly ComponentResourceKey MonitorAvailableIconKey =
              new ComponentResourceKey(typeof(TrayIconViewModel), "ApplicationIcon");
      
      public static readonly ComponentResourceKey MonitorNotAvailableIconKey =
              new ComponentResourceKey(typeof(TrayIconViewModel), "MonitorNotAvailableIcon");
      
      #endregion

      #region Logger Images

      public static readonly ComponentResourceKey LoggerWarnImageKey =
              new ComponentResourceKey(typeof(ResourceKeys),
                                       "LoggerWarnImage");

      public static readonly ComponentResourceKey LoggerDebugImageKey =
          new ComponentResourceKey(typeof(ResourceKeys),
                                   "LoggerDebugImage");

      public static readonly ComponentResourceKey LoggerTraceImageKey =
          new ComponentResourceKey(typeof(ResourceKeys),
                                   "LoggerTraceImage");

      public static readonly ComponentResourceKey LoggerInfoImageKey =
          new ComponentResourceKey(typeof(ResourceKeys),
                                   "LoggerInfoImage");

      public static readonly ComponentResourceKey LoggerFatalImageKey =
          new ComponentResourceKey(typeof(ResourceKeys),
                                   "LoggerImageFatal");

      public static readonly ComponentResourceKey LoggerErrorImageKey =
          new ComponentResourceKey(typeof(ResourceKeys),
                                   "LoggerImageError");

      public static readonly ComponentResourceKey LoggerExceptionImageKey =
          new ComponentResourceKey(typeof(ResourceKeys),
                                   "LoggerImageExceptionKey");

      public static readonly ComponentResourceKey LoggerExceptionMonoImageKey =
          new ComponentResourceKey(typeof(ResourceKeys),
                                   "LoggerImageExceptionMonoKey");
      
      #endregion

    }
}
