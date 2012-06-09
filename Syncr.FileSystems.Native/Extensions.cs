using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syncr.FileSystems.Native
{
    public static class Extensions
    {
        internal static string WithTrailingPathSeparator(this string input)
        {
            if (input.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return input;
            else
                return input + Path.DirectorySeparatorChar.ToString();
        }

        internal static string WithoutTrailingPathSeparator(this string input)
        {
            if (input.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return input.Substring(0, input.Length - 1);
            else
                return input;
        }
    }
}
