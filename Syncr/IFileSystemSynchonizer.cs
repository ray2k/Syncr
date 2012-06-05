using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface IFileSystemSynchronizer
    {
        void Start(IFileSystem localTarget, IFileSystem remoteSource, SyncronizationOptions options);
        void Stop();
        IFileSystem Source { get; }
        IFileSystem Destination { get; }
        SyncronizationOptions Options { get; }
        event EventHandler<FileSystemUpdatedEventArgs> FileSystemUpdated;
    }
}
