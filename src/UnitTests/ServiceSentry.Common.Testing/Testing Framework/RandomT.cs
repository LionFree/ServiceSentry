// -----------------------------------------------------------------------
//  <copyright file="RandomT.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Win32;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    partial class Tests
    {
        /// <summary>
        ///     Creates a random element of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The enum from which to select a random element. T can be on of:
        ///     an <see cref="Enum" />,
        ///     a <see cref="string" />,
        ///     an <see cref="int" />,
        ///     a <see cref="long" />,
        ///     a <see cref="double" />,
        ///     a <see cref="bool" />,
        ///     a <see cref="Microsoft.Win32.RegistryKey" /> or
        ///     a <see cref="DateTime" />.
        /// </typeparam>
        /// <returns>A random element from the supplied enum.</returns>
        [DebuggerStepThrough]
        public static T Random<T>()
        {
            var objType = typeof (T);

            if (objType.IsEnum)
            {
                var values = Enum.GetValues(objType);
                return (T) values.GetValue(Randomizer.Next(values.Length));
            }
            if (objType == typeof (String))
            {
                return (T) (object) RandomString();
            }

            if (objType == typeof (int))
            {
                return (T) (object) RandomInt();
            }

            if (objType == typeof(char))
            {
                return (T)(object) RandomChar();
            }


            if (objType == typeof (long))
            {
                return (T) (object) RandomLong();
            }

            if (objType == typeof (double))
            {
                return (T) (object) RandomDouble();
            }

            if (objType == typeof (bool))
            {
                return (T) (object) (Randomizer.Next(2) == 1);
            }

            if (objType == typeof (DateTime))
            {
                return (T) (object) RandomDateTime();
            }

            if (objType == typeof (RegistryKey))
            {
                return (T) (object) RandomRegistryKey();
            }

            if (objType == typeof (Service))
            {
                return (T) (object) RandomService();
            }

            if (objType == typeof(Exception))
            {
                return (T)(object)RandomException();
            }

            throw new ArgumentException(
                "Type T must be string, int, long, double, bool, DateTime, RegistryKey, or an Enum.");
        }

        [DebuggerStepThrough]
        public static T Random<T>(int min, int max)
        {
            if (typeof (T) != typeof (int)) throw new ArgumentException("Type T must be int");
            return (T) (object) RandomInt(min, max);
        }

        [DebuggerStepThrough]
        public static T Random<T>(long min, long max)
        {
            if (typeof (T) != typeof (long)) throw new ArgumentException("Type T must be long.");
            return (T) (object) RandomLong(min, max);
        }

        [DebuggerStepThrough]
        public static T Random<T>(double min, double max)
        {
            if (typeof (T) != typeof (double)) throw new ArgumentException("Type T must be double.");
            return (T) (object) RandomDouble(min, max);
        }

        [DebuggerStepThrough]
        public static T Random<T>(int max)
        {
            var objType = typeof (T);
            if (objType == typeof (String))
            {
                return (T) (object) RandomString(max);
            }
            if (objType == typeof (int))
            {
                return (T) (object) RandomInt(0, max);
            }
            throw new ArgumentException("Type T must be string or int.");
        }

        [DebuggerStepThrough]
        public static T Random<T>(long max)
        {
            if (typeof (T) != typeof (long)) throw new ArgumentException("Type T must be long.");
            return (T) (object) RandomLong(0, max);
        }

        [DebuggerStepThrough]
        public static T Random<T>(double max)
        {
            if (typeof (T) != typeof (double)) throw new ArgumentException("Type T must be double.");
            return (T) (object) RandomDouble(0, max);
        }


        /// <summary>
        ///     Given an element from an enumeration of type T,
        ///     gets a different element.
        /// </summary>
        /// <typeparam name="T">The enum from which to select a random element.</typeparam>
        /// <param name="oldValue">The current element of the enum.</param>
        /// <returns>The next (or first) enum in the list.</returns>
        [DebuggerStepThrough]
        public static T NewRandom<T>(T oldValue) where T : struct, IConvertible
        {
            if (!typeof (T).IsEnum) throw new ArgumentException("T must be an enumerated type.");

            var values = Enum.GetValues(typeof (T));

            var current = (int) Enum.Parse(typeof (T), oldValue.ToString(CultureInfo.CurrentCulture));
            var next = (current + 1)%values.Length;

            var output = (T) values.GetValue(next);

            return output;
        }

        [DebuggerStepThrough]
        public static List<T> RandomList<T>(int minLength = 0, int maxLength = 500)
        {
            var length = Randomizer.Next(minLength, maxLength + 1);
            var output = new List<T>();
            for (var i = 0; i < length; i++)
            {
                output.Add(Random<T>());
            }
            return output;
        }
    }
}