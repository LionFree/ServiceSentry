// -----------------------------------------------------------------------
//  <copyright file="FileNameFormattingOption.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

#endregion

namespace ServiceSentry.Extensibility.Logging
{
    [DataContract, KnownType(typeof (FNFOptionImplementation))]
    public abstract class FileNameFormattingOption
    {
        /// <summary>
        ///     The name of the assembly.
        /// </summary>
        public static readonly FileNameFormattingOption AssemblyName
            = new FNFOptionImplementation("AssemblyName", string.Empty, () => AssemblyInspector.Default.HasEntryAssembly ? 
                                                                                  AssemblyWrapper.Default.GetEntryAssembly().GetName().Name : string.Empty);
        
        /// <summary>
        ///     The date, formatted in the sortable date pattern:
        ///     6/15/2009 1:45:30 PM => 2009-06-15
        /// </summary>
        public static readonly FileNameFormattingOption SortableDate
            = new FNFOptionImplementation("SortableDate", String.Format("{0:yyyy-MM-dd}", DateTime.Today), null);

        /// <summary>
        ///     The hours and minutes offset from UTC:
        ///     6/15/2009 1:45:30 PM -07:00 => -07.00
        /// </summary>
        public static readonly FileNameFormattingOption UTCOffset
            = new FNFOptionImplementation("UTCOffset",
                                          String.Format("{0:zzz}", DateTime.Today).Replace('/', '.').Replace(':', '.'),
                                          null);

        public static FileNameFormattingOption CustomName(string name)
        {
            return new FNFOptionImplementation("CustomName", name, null);
        }

        public static ObservableCollection<FileNameFormattingOption> AllOptions
        {
            get
            {
                return new ObservableCollection<FileNameFormattingOption>
                    {
                        AssemblyName,
                        SortableDate,
                        UTCOffset,
                    };
            }
        }

        public static ObservableCollection<string> AllOptionNames
        {
            get
            {
                var output = new ObservableCollection<string>();
                foreach (var item in AllOptions)
                {
                    output.Add(item.Name);
                }
                return output;
            }
        }

        #region Abstract Members

        /// <summary>
        ///     Gets the name of the <see cref="FileNameFormattingOption" />.
        /// </summary>
        [DataMember]
        public abstract string Name { get; }

        /// <summary>
        ///     Gets the string value of the <see cref="FileNameFormattingOption" />.
        /// </summary>
        [DataMember]
        public abstract string Format { get; }

        public static FileNameFormattingOption ByName(string name)
        {
            foreach (var item in AllOptions)
            {
                if (item.Name == name) return item;
            }

            return null;
        }

        #endregion

        private sealed class FNFOptionImplementation : FileNameFormattingOption
        {
            private readonly string _format;
            private readonly Func<string> _function;
            private readonly string _name;

            /// <summary>
            ///     Returns a new instance of the <see cref="FileNameFormattingOption" /> class,
            ///     using the indicated options.
            /// If <paramref name="function"/> is null, the <paramref name="format"/> string will be used.
            /// </summary>
            /// <param name="name">A string representing the name of the option.</param>
            /// <param name="format">A string representing the value to return.</param>
            /// <param name="function">A <see cref="Func{T}"/> that will produce the <see cref="string"/> value to return.</param>
            internal FNFOptionImplementation(string name, string format, Func<string> function)
            {
                _name = name;
                _format = format;
                _function = function;
            }

            public override string Name
            {
                get { return _name; }
            }

            public override string Format
            {
                get { return (_function == null) ? _format : _function.Invoke(); }
            }
        }
    }
}