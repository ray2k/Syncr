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
    }
}
