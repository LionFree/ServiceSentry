// -----------------------------------------------------------------------
//  <copyright file="RandomService.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;
using Moq;
using ServiceSentry.Model;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        private static Service RandomService()
        {
            var output = new Mock<Service>();
            output.Setup(mock => mock.Guid).Returns(Guid.NewGuid);
            return output.Object;
        }
    }
}