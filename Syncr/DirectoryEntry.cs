using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Syncr
{
    public abstract class DirectoryEntry : FileSystemEntry
    {
        public override bool IsInConflictWith(FileSystemEntry otherEntry)
        {
            return false;
        }        
    }
}
