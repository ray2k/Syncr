using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public class LinuxFileSystemOptions
    {
        public LinuxFileSystemOptions()
        {
        }

        [Category("General")]
        [Description("The full local")]
        public string Path { get; set; }
    }
}
