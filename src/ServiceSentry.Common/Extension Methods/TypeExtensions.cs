// -----------------------------------------------------------------------
//  <copyright file="TypeExtensions.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ServiceSentry.ServiceFramework
{
    /// <summary>
    ///     Helper functions to get <see cref="Attribute" />s from <see cref="System.Type" />s.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Queries a <see cref="System.Type" /> for a list of all <see cref="Attribute" />s of a specific type.
        /// </summary>
        /// <param name="typeWithAttributes">
        ///     The <see cref="Type" /> to query.
        /// </param>
        /// <typeparam name="T">
        ///     The <see cref="Type" /> of <see cref="Attribute" /> to search for.
        /// </typeparam>
        /// <returns>
        ///     A <see cref="System.Collections.Generic.IEnumerable{T}" /> list of all <see cref="Attribute" />s of the desired type on the
        ///     <see
        ///         cref="System.Type" />
        ///     .
        /// </returns>
        public static IEnumerable<T> GetAttributes<T>(this Type typeWithAttributes) where T : Attribute
        {
            // Try to find the configuration attribute for the 
            // default logger if it exists
            var configAttributes = Attribute.GetCustomAttributes(typeWithAttributes, typeof (T), false);

            // get just the first one
            if (configAttributes.Length > 0)
            {
                foreach (T attribute in configAttributes)
                {
                    yield return attribute;
                }
            }
        }

        /// <summary>
        ///     Queries a <see cref="System.Type" /> for the first <see cref="Attribute" /> of a specific type.
        /// </summary>
        /// <param name="typeWithAttributes">
        ///     The <see cref="Type" /> to query.
        /// </param>
        /// <typeparam name="T">
        ///     The <see cref="Type" /> of <see cref="Attribute" /> to search for.
        /// </typeparam>
        /// <returns>
        ///     The first <see cref="Attribute" /> of the desired type on the <see cref="System.Type" />.
        /// </returns>
        public static T GetAttribute<T>(this Type typeWithAttributes) where T : Attribute
        {
            return GetAttributes<T>(typeWithAttributes).FirstOrDefault();
        }
    }
}