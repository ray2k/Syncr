using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public sealed class FileSystemUpdateFailedEventArgs : EventArgs
    {
        private FileSystemUpdateFailedEventArgs()
        {
        }

        internal FileSystemUpdateFailedEventArgs(ISyncProvider fileSystem, FileSystemChange change, Exception exception)
        {
            this.FileSystem = fileSystem;
            this.Entry = change.Entry;
            this.ChangeType = change.ChangeType;
            this.Exception = exception;
        }

        public FileSystemEntry Entry { get; private set; }
        public FileSystemChangeType ChangeType { get; private set; }
        public ISyncProvider FileSystem { get; private set; }
        public Exception Exception { get; private set; }
    }
}
