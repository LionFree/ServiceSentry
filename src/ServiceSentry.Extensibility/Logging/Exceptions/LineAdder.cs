// -----------------------------------------------------------------------
//  <copyright file="ExceptionFormatter.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Collections.Generic;
using System.Linq;

#endregion

namespace ServiceSentry.Extensibility.Logging.Exceptions
{
    internal abstract class LineAdder
    {
        /// <summary>
        ///     Creates a new instance of the class, using the default values.
        /// </summary>
        public static LineAdder Default
        {
            get { return new LineAdderImplementation(); }
        }

        /// <summary>
        ///     Adds the string to the list of strings, broken across linebreaks.
        /// </summary>
        /// <param name="strings">The list of strings.</param>
        /// <param name="message">The string to add to the list.</param>
        internal abstract void AddLines(List<string> strings, string message);

        private sealed class LineAdderImplementation : LineAdder
        {
            internal override void AddLines(List<string> strings, string message)
            {
                var lines = message.Split('\n');

                strings.Add(lines[0].Trim('\r'));

                foreach (var line in lines.Skip(1))
                {
                    strings.Add(line.Trim('\r'));
                }
            }
        }
    }
}