using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public class NativeSyncProviderFactory : ISyncProviderFactory
    {
        public ISyncProvider CreateFileSystem(object options)
        {
            if (options is WindowsFileSystemOptions)
                return new NativeSyncProvider(options as WindowsFileSystemOptions);
            else if (options is LinuxFileSystemOptions)
                return new NativeSyncProvider(options as LinuxFileSystemOptions);
            else 
                throw new InvalidOperationException("Expected NativeFileSystemOptions or LinuxFileSystemOptions");
        }

        public object CreateDefaultOptions()
        {
            return NativeFileSystemOptionsFactory.Create();
        }

        public string Name
        {
            get { return "Native FileSystem"; }
        }

        public string Description
        {
            get { return "Allows syncing to or from a local or UNC filesystem"; }
        }
    }
}
