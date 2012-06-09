using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Syncr.FileSystems.Native
{
    public sealed class NativeDirectoryEntry : DirectoryEntry
    {
        public string BaseDirectory { get; set; }

        private FileInfoBase DirectoryInfo { get; set; }

        public NativeDirectoryEntry(FileInfoBase directoryInfo)
        {
            this.DirectoryInfo = directoryInfo;
        }

        public override bool CanWriteCreationTime
        {
            get { return true; }
        }

        public override bool CanWriteModificationTime
        {
            get { return true; }
        }

        protected override void WriteCreationTime(DateTime createdUtc)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);

            this.DirectoryInfo.CreationTimeUtc = createdUtc;
        }

        protected override void WriteModificationTime(DateTime modifiedUtc)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);
            this.DirectoryInfo.LastWriteTimeUtc = modifiedUtc;
        }
    }
}
