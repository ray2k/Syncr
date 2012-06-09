using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface IFileSystemSynchronizer
    {
        void Start(ISyncProvider localTarget, ISyncProvider remoteSource, SyncronizationOptions options);
        void Stop();
        ISyncProvider Source { get; }
        ISyncProvider Destination { get; }
        SyncronizationOptions Options { get; }
        event EventHandler<FileSystemUpdatedEventArgs> FileSystemUpdated;
    }
}
