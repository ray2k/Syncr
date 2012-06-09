using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public sealed class NativeFileEntry : FileEntry
    {
        private FileInfoBase FileInfo { get; set; }
        
        public string BaseDirectory { get; set; }

        public NativeFileEntry(FileInfoBase fileInfo)
        {
            this.FileInfo = fileInfo;
        }       

        public override System.IO.Stream Open()
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);

            return this.FileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public override bool CanWriteCreationTime
        {
            get { return true; }
        }

        public override bool CanWriteModificationTime
        {
            get { return true; }
        }

        protected override void WriteCreationTime(DateTime created)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);

            this.FileInfo.CreationTimeUtc = created;
        }

        protected override void WriteModificationTime(DateTime modified)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);
            this.FileInfo.LastWriteTimeUtc = modified;
        }
    }
}
