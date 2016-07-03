// -----------------------------------------------------------------------
//  <copyright file="TestAssembly.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System.Reflection;
using System.Runtime.Serialization;

#endregion

namespace ServiceSentry.Common.Testing
{
    // The assembly class doesn't define a deserializer, so we need to implement one
    // in order to mock an assembly.
    public abstract class TestAssembly : Assembly
    {
        public new abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}