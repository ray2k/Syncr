using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr.FileSystems.Native
{
    internal static class Runtime
    {
        internal static bool IsLinux()
        {
            int p = (int) Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
