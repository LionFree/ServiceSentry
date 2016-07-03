// -----------------------------------------------------------------------
//  <copyright file="FileNameFormatter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.Extensibility.Logging
{
    public abstract class FileNameFormatter
    {
        public static FileNameFormatter Default
        {
            get { return new FNFImplementation(); }
        }

        /// <summary>
        ///     Replaces the format items in a specified string with the string representations
        ///     of corresponding <see cref="FileNameFormattingOption" /> in a specified array.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        ///     An <see cref="FileNameFormattingOption" /> array that contains zero or more <see cref="FileNameFormattingOption" />s to format.
        /// </param>
        /// <returns>
        ///     If <paramref name="format" /> is empty, returns a string in the format
        ///     "<c>assembly name</c>.<c>date</c>.log".
        ///     If <paramref name="args" /> is null, returns <paramref name="format" />.
        ///     Otherwise, returns the composite format string with the specified replacements.
        /// </returns>
        public abstract string Format(string format, params FileNameFormattingOption[] args);

        private sealed class FNFImplementation : FileNameFormatter
        {
            public override string Format(string format, params FileNameFormattingOption[] args)
            {
                if (string.IsNullOrEmpty(format))
                {
                    return String.Format("{0}.{1}.log",
                                         FileNameFormattingOption.AssemblyName.Format,
                                         FileNameFormattingOption.SortableDate.Format);
                }

                if (args == null) return format;

                var formatArgs = new object[args.Length];
                {
                    for (var i = 0; i < args.Length; i++)
                    {
                        formatArgs[i] = args[i].Format;
                    }
                }
                return string.Format(format, formatArgs);
            }
        }
    }
}