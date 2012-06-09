using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public class NativeFileSystemProvider : ISyncProviderFactory
    {
        public ISyncProvider CreateFileSystem(object options)
        {
            var nfso = options as WindowsFileSystemOptions;
            if (nfso == null)
                throw new InvalidOperationException("Expected NativeFileSystemOptions");

            return new NativeSyncProvider(nfso);
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
