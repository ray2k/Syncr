using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public sealed class FileSystemUpdatedEventArgs : EventArgs
    {
        private FileSystemUpdatedEventArgs()
        {
        }

        internal FileSystemUpdatedEventArgs(IFileSystem fileSystem, FileSystemChange change)
        {
            this.FileSystem = fileSystem;
            this.Entry = change.Entry;
            this.ChangeType = change.ChangeType;
        }

        public FileSystemEntry Entry { get; private set; }
        public FileSystemChangeType ChangeType { get; private set; }
        public IFileSystem FileSystem { get; private set; }        
    }
}
