// -----------------------------------------------------------------------
//  <copyright file="PathShortener.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace ServiceSentry.Extensibility
{
    public abstract class PathShortener
    {
        internal static double ControlWidth = 20.0;
        internal static Regex PrevWord = new Regex(@"\W*\w*$");
        internal static Regex NextWord = new Regex(@"\w*\W*");

        public static PathShortener GetInstance(EllipsisFormat options)
        {
            return new ShortenerImplementation(options);
        }

        public abstract string Compact(string path, TextBlock textBlock, double width);

        private sealed class ShortenerImplementation : PathShortener
        {
            private readonly EllipsisFormat _options;

            public ShortenerImplementation(EllipsisFormat options)
            {
                _options = options;
            }

            public override string Compact(string path, TextBlock control, double width)
            {
                var padding = control.Padding.Left + control.Padding.Right + ControlWidth;
                width -= padding;

                var typeFace = new Typeface(control.FontFamily, control.FontStyle, control.FontWeight,
                                            control.FontStretch);

                var formattedText = new FormattedText(path,
                                                      CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                                                      typeFace,
                                                      control.FontSize, control.Foreground);

                if (formattedText.Width < width) return path; // The desired width will accommodate the whole path

                // split path into <drive><directory><filename>
                var pre = Path.GetPathRoot(path);
                if (pre == null) throw new ArgumentException(Strings.EXCEPTION_PathRootIsNull, "path");

                var directory = Path.GetDirectoryName(path);
                if (directory == null) throw new ArgumentException(Strings.EXCEPTION_PathDirectoryIsNull, "path");

                var mid = directory.Substring(pre.Length);
                var post = Path.GetFileName(path);
                if (post == null) throw new ArgumentException(Strings.EXCEPTION_FilenameIsNull, "path");

                var len = 0;
                var seg = mid.Length;
                var fit = string.Empty;

                // find the longest string that fits into the
                // boundaries using the bisection method.
                while (seg > 1)
                {
                    seg -= seg/2;
                    var left = len + seg;
                    var right = mid.Length;
                    if (left > right) continue;

                    if ((EllipsisFormat.Middle & _options) ==
                        EllipsisFormat.Middle)
                    {
                        right -= left/2;
                        left -= left/2;
                    }
                    else if ((EllipsisFormat.Start & _options) != 0)
                    {
                        right -= left;
                        left = 0;
                    }

                    // build and measure a candidate string with ellipsis
                    var txt = mid.Substring(0, left) + Strings.Noun_EllipsisChars + mid.Substring(right);

                    // restore path with <drive> and <filename>
                    txt = Path.Combine(Path.Combine(pre, txt), post);

                    formattedText = new FormattedText(txt,
                                                      CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                                                      typeFace,
                                                      control.FontSize, control.Foreground);

                    // candidate string fits into control boundaries, 
                    // try a longer string
                    // stop when seg <= 1 
                    if (!(formattedText.Width <= width)) continue;
                    len += seg;
                    fit = txt;
                }

                if (len == 0) // string can't fit into control
                {
                    // <drive> and <directory> are empty, return <filename>
                    if (pre.Length == 0 && mid.Length == 0)
                        return post;

                    fit = Path.Combine(Path.Combine(pre, Strings.Noun_EllipsisChars), post);
                    formattedText = new FormattedText(fit,
                                                      CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                                                      typeFace,
                                                      control.FontSize, control.Foreground);
                    if (formattedText.Width > width)
                        fit = Path.Combine(Strings.Noun_EllipsisChars, post);
                }
                return fit;
            }
        }
    }

    [Flags]
    public enum EllipsisFormat
    {
        // Text is not modified.
        None = 0,
        // Text is trimmed at the end of the string. An ellipsis (...) 
        // is drawn in place of remaining text.
        End = 1,
        // Text is trimmed at the beginning of the string. 
        // An ellipsis (...) is drawn in place of remaining text. 
        Start = 2,
        // Text is trimmed in the middle of the string. 
        // An ellipsis (...) is drawn in place of remaining text.
        Middle = 3
    }
}