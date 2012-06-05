using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface IFileSystemUpdater
    {
        void ApplyChangesWhile(IFileSystem fileSystem, IEnumerable<FileSystemChange> changes, Func<bool> stopCheck);
        event EventHandler<FileSystemUpdatedEventArgs> FileSystemUpdated;
        event EventHandler<FileSystemUpdateFailedEventArgs> FileSystemUpdateFailed;
    }
}
