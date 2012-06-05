using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public class FileSystemUpdater : IFileSystemUpdater
    {
        public void ApplyChangesWhile(IFileSystem fileSystem, IEnumerable<FileSystemChange> changes, Func<bool> stopCheck)
        {
            foreach (var change in changes)
            {
                if (stopCheck() == true)
                    break;

                FileSystemEntry newEntry = null;                
                
                try
                {                    
                    if (change.ChangeType == FileSystemChangeType.Create)
                    {
                        newEntry = fileSystem.Create(change.Entry);
                    }
                    else if (change.ChangeType == FileSystemChangeType.Delete)
                    {
                        fileSystem.Delete(change.Entry);
                    }
                    else if (change.ChangeType == FileSystemChangeType.Overwrite)
                    {
                        fileSystem.Delete(change.Entry);
                        newEntry = fileSystem.Create(change.Entry);
                    }

                    if (newEntry != null && change.Entry.Created != null)
                        newEntry.SetCreationTime(change.Entry.Created.Value);

                    if (newEntry != null && change.Entry.Modified != null)
                        newEntry.SetModificationTime(change.Entry.Modified.Value);

                    NotifyFileSystemUpdated(fileSystem, change);
                }
                catch (Exception ex)
                {
                    NotifyFileSystemUpdateFailed(fileSystem, change, ex);
                }                
            }
        }

        private void NotifyFileSystemUpdateFailed(IFileSystem fileSystem, FileSystemChange change, Exception ex)
        {
            if (FileSystemUpdateFailed != null)
                FileSystemUpdateFailed(this, new FileSystemUpdateFailedEventArgs(fileSystem, change, ex));     
        }

        private void NotifyFileSystemUpdated(IFileSystem fileSystem, FileSystemChange change)
        {
            if (FileSystemUpdated != null)
                FileSystemUpdated(this, new FileSystemUpdatedEventArgs(fileSystem, change));                
        }

        public event EventHandler<FileSystemUpdatedEventArgs> FileSystemUpdated;
        public event EventHandler<FileSystemUpdateFailedEventArgs> FileSystemUpdateFailed;
    }
}
