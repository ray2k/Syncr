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
        private IFileInfoFactory FileInfoFactory { get; set; }
        
        public string BaseDirectory { get; set; }

        public NativeFileEntry(IFileInfoFactory fileInfoFactory)
        {
            this.FileInfoFactory = fileInfoFactory;
        }       

        public override System.IO.Stream Open()
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);

            return this.FileInfoFactory.FromFileName(fullPath).Open(FileMode.Open, FileAccess.Read, FileShare.Read);
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

            this.FileInfoFactory.FromFileName(fullPath).CreationTimeUtc = created;
        }

        protected override void WriteModificationTime(DateTime modified)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);
            this.FileInfoFactory.FromFileName(fullPath).LastWriteTimeUtc = modified;
        }
    }
}
