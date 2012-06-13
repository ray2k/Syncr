using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface ISyncProvider : IDisposable
    {
        IEnumerable<FileSystemEntry> GetFileSystemEntries(SearchOption searchOption);
        FileSystemEntry Create(FileSystemEntry entry);
        void Delete(FileSystemEntry entry);        
        string Id { get; set; }        
    }
}
