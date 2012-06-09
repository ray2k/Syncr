using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public abstract class SyncProviderBase : ISyncProvider
    {
        ~SyncProviderBase()
        {
            Dispose(false);
        }

        public abstract IEnumerable<FileSystemEntry> GetFileSystemEntries(System.IO.SearchOption seachOption);
        public abstract FileSystemEntry Create(FileSystemEntry entry);
        public abstract void Delete(FileSystemEntry entry);
        public string Id { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }
    }
}
