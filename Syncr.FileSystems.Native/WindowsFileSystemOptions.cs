using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public class WindowsFileSystemOptions
    {
        public WindowsFileSystemOptions()
        {
        }

        [Category("General")]
        [Description("The full local or UNC path")]
        public string Path { get; set; }

        [Category("Network Path")]
        [Description("User name to use when accessing the remote file system")]
        public string UserName { get; set; }

        [Category("Network Path")]
        [Description("Password to use when accessing the remote file system")]
        public string Password { get; set; }
    }
}
