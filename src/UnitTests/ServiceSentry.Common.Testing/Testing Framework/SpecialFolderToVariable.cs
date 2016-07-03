// -----------------------------------------------------------------------
//  <copyright file="SpecialFolderToVariable.cs" company="Curtis Kaler">
//      Copyright (c) 2014 Curtis Kaler  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#region References

using System;

#endregion

namespace ServiceSentry.UnitTests.Common
{
    public partial class Tests
    {
        public static string SpecialFolderToVariable(Environment.SpecialFolder input)
        {
            switch (input)
            {
                case Environment.SpecialFolder.ProgramFiles:
                    return "%PROGRAMFILES%";

                case Environment.SpecialFolder.UserProfile:
                    return "%USERPROFILE%";

                case Environment.SpecialFolder.LocalApplicationData:
                    return "%LOCALAPPDATA%";

                case Environment.SpecialFolder.Windows:
                    return "%WINDIR%";
            }
            return string.Empty;
        }
    }
}