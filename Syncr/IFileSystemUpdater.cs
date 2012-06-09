using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface IFileSystemUpdater
    {
        void ApplyChangesWhile(ISyncProvider fileSystem, IEnumerable<FileSystemChange> changes, Func<bool> stopCheck);
        event EventHandler<FileSystemUpdatedEventArgs> FileSystemUpdated;
        event EventHandler<FileSystemUpdateFailedEventArgs> FileSystemUpdateFailed;
    }
}
