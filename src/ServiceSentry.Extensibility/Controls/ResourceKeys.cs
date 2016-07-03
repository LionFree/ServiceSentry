// -----------------------------------------------------------------------
//  <copyright file="ResourceKeys.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Windows;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public static class ResourceKeys
    {
        #region Logger Images

        public static readonly ComponentResourceKey LoggerWarnImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerWarnImage");

        public static readonly ComponentResourceKey LoggerDebugImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerDebugImage");

        public static readonly ComponentResourceKey LoggerTraceImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerTraceImage");

        public static readonly ComponentResourceKey LoggerInfoImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerInfoImage");

        public static readonly ComponentResourceKey LoggerFatalImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerImageFatal");

        public static readonly ComponentResourceKey LoggerErrorImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerImageError");

        public static readonly ComponentResourceKey LoggerExceptionImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerImageExceptionKey");

        public static readonly ComponentResourceKey LoggerExceptionMonoImageKey =
            new ComponentResourceKey(typeof (ResourceKeys), "LoggerImageExceptionMonoKey");

        #endregion

        #region Colors

        public static readonly ComponentResourceKey GlyphBrushKey =
            new ComponentResourceKey(typeof (ResourceKeys), "GlyphBrushKey");

        public static readonly ComponentResourceKey ControlDisabledForegroundKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ControlDisabledForegroundKey");

        public static readonly ComponentResourceKey ControlDisabledBackgroundKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ControlDisabledBackgroundKey");

        public static readonly ComponentResourceKey ButtonNormalOuterBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonNormalOuterBorderKey");

        public static readonly ComponentResourceKey ButtonNormalInnerBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonNormalInnerBorderKey");

        public static readonly ComponentResourceKey ButtonNormalBackgroundKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonNormalBackgroundKey");

        public static readonly ComponentResourceKey ButtonMouseOverBackgroundKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonMouseOverBackgroundKey");

        public static readonly ComponentResourceKey ButtonMouseOverOuterBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonMouseOverOuterBorderKey");

        public static readonly ComponentResourceKey ButtonMouseOverInnerBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonMouseOverInnerBorderKey");

        public static readonly ComponentResourceKey ButtonPressedOuterBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonPressedOuterBorderKey");

        public static readonly ComponentResourceKey ButtonPressedInnerBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonPressedInnerBorderKey");

        public static readonly ComponentResourceKey ButtonPressedBackgroundKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonPressedBackgroundKey");

        public static readonly ComponentResourceKey ButtonFocusedOuterBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonFocusedOuterBorderKey");

        public static readonly ComponentResourceKey ButtonFocusedInnerBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonFocusedInnerBorderKey");

        public static readonly ComponentResourceKey ButtonFocusedBackgroundKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonFocusedBackgroundKey");

        public static readonly ComponentResourceKey ButtonDisabledOuterBorderKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonDisabledOuterBorderKey");

        public static readonly ComponentResourceKey ButtonInnerBorderDisabledKey =
            new ComponentResourceKey(typeof (ResourceKeys), "ButtonInnerBorderDisabledKey");

        #endregion
    }
}