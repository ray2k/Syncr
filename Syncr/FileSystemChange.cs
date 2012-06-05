using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public enum FileSystemChangeType
    {
        Create = 0,
        Overwrite = 1,
        Delete = 2,
    }

    public class FileSystemChange
    {
        public FileSystemEntry Entry { get; private set; }
        public FileSystemChangeType ChangeType { get; private set; }

        private FileSystemChange()
        {
        }

        public static FileSystemChange ForCreate(FileSystemEntry entry)
        {
            return new FileSystemChange()
            {
                Entry = entry,
                ChangeType = FileSystemChangeType.Create
            };
        }

        public static FileSystemChange ForDelete(FileSystemEntry entry)
        {
            return new FileSystemChange()
            {
                Entry = entry,
                ChangeType = FileSystemChangeType.Delete
            };
        }

        public static FileSystemChange ForOverwrite(FileSystemEntry entry)
        {
            return new FileSystemChange()
            {
                Entry = entry,
                ChangeType = FileSystemChangeType.Overwrite
            };
        }
    }
}
