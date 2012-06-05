using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface IFileSystem
    {
        IEnumerable<FileSystemEntry> GetFileSystemEntries(SearchOption seachOption);
        FileSystemEntry Create(FileSystemEntry entry);
        void Delete(FileSystemEntry entry);        
        string Id { get; set; }        
    }
}
