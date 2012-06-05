using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public static class NativeFileSystemOptionsFactory
    {
        public static object Create(Func<bool> linuxDetector)
        {
            if (linuxDetector() == true)
                return new LinuxFileSystemOptions();
            else
                return new WindowsFileSystemOptions();
        }

        public static object Create()
        {
            return Create(() => Runtime.IsLinux());
        }
    }
}
