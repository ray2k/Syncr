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
        [Description("Name of the user account to access the remote file system")]
        public string UserName { get; set; }

        [Category("Network Path")]
        [Description("Password for the user account to access the remote file system")]
        public string Password { get; set; }
    }
}
