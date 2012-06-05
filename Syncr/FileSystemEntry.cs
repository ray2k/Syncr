using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syncr
{
    public abstract class FileSystemEntry : IDisposable
    {
        protected FileSystemEntry()
        {
        }

        ~FileSystemEntry()
        {
            Dispose(false);
        }

        public string Name { get; set; }
        public string RelativePath { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }        
        public abstract bool IsInConflictWith(FileSystemEntry otherEntry);

        public abstract bool CanWriteCreationTime { get; }
        public abstract bool CanWriteModificationTime { get; }

        public void SetCreationTime(DateTime created)
        {
            if (CanWriteCreationTime)
                WriteCreationTime(created);
        }

        protected virtual void WriteCreationTime(DateTime created)
        {
        }

        public void SetModificationTime(DateTime created)
        {
            if (CanWriteCreationTime)
                WriteModificationTime(created);
        }

        protected virtual void WriteModificationTime(DateTime created)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }        
    }
}
