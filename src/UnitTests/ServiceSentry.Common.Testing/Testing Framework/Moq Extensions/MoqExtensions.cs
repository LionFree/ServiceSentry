// -----------------------------------------------------------------------
//  <copyright file="MoqExtensions.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using System.Collections.Generic;
using Moq.Language.Flow;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public static class MoqExtensions
    {
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
                                                      params TResult[] results) where T : class
        {
            setup.Returns(new Queue<TResult>(results).Dequeue);
        }

        public static bool Contains(this string[] array, string stringName)
        {
            var position = Array.IndexOf(array, stringName);
            return position > -1;
        }
    }
}