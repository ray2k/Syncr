using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr
{
    public abstract class FileEntry : FileSystemEntry
    {
        public abstract Stream Open();

        public virtual long Size { get; set; }

        public sealed override bool IsInConflictWith(FileSystemEntry otherEntry)
        {
            if (otherEntry is FileEntry)
                return IsInConflictWith(otherEntry as FileEntry);

            throw new InvalidOperationException("Can only compare FileEntry instances to other FileEntry instances");
        }

        public virtual bool IsInConflictWith(FileEntry fileEntry)
        {
            if (fileEntry.Size != this.Size)
                return true; // always a conflict if size doesn't match

            if (fileEntry.CanWriteModificationTime == true && this.CanWriteModificationTime == true)
                return (fileEntry.Modified != this.Modified);

            if (fileEntry.CanWriteCreationTime == true && this.CanWriteCreationTime == true)
                return (fileEntry.Created != this.Created);

            return false;
        }
    }
}
