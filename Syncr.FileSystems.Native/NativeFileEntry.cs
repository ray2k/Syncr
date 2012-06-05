using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public sealed class NativeFileEntry : FileEntry
    {
        public string BaseDirectory { get; set; }

        public override System.IO.Stream Open()
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);

            return File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
            new FileInfo(fullPath).CreationTimeUtc = created;
        }

        protected override void WriteModificationTime(DateTime created)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);
            new FileInfo(fullPath).LastWriteTimeUtc = created;
        }
    }
}
