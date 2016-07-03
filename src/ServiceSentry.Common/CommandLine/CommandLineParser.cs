// -----------------------------------------------------------------------
//  <copyright file="CommandLineParser.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Specialized;

#endregion

namespace ServiceSentry.Common.CommandLine
{
    public abstract class CommandLineParser
    {
        public abstract int Length { get; }
        public abstract string[] Keys { get; }
        public abstract string[] Values { get; }
        public abstract string this[string parameter] { get; }
        public abstract CommandLineParser Parse(string[] args);

        public static CommandLineParser GetInstance()
        {
            return new Implementation();
        }

        private sealed class Implementation : CommandLineParser
        {
            private readonly StringDictionary _parameters;

            internal Implementation()
            {
                _parameters = new StringDictionary();
            }

            public override int Length
            {
                get { return (_parameters.Count); }
            }

            public override string this[string parameter]
            {
                get { return (_parameters[parameter]); }
            }

            public override string[] Keys
            {
                get
                {
                    var output = new String[_parameters.Count];
                    _parameters.Keys.CopyTo(output, 0);
                    return output;
                }
            }

            public override string[] Values
            {
                get
                {
                    var output = new String[_parameters.Count];
                    _parameters.Values.CopyTo(output, 0);
                    return output;
                }
            }

            public override CommandLineParser Parse(string[] args)
            {
                // Valid format:
                // {switchCharacter}{switch}{delimiter}[quote]{parameter}[quote]

                if (_parameters.Count != 0) _parameters.Clear();

                foreach (var text in args)
                {
                    var arg = CommandLineArgument.GetInstance(text);
                    _parameters.Add(arg.Switch, arg.HasParameter ? arg.Parameter : true.ToString());
                }

                return this;
            }
        }
    }
}