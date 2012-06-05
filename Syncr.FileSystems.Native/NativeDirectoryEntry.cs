using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syncr.FileSystems.Native
{
    public sealed class NativeDirectoryEntry : DirectoryEntry
    {
        public string BaseDirectory { get; set; }

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
            new DirectoryInfo(fullPath).CreationTimeUtc = created;
        }

        protected override void WriteModificationTime(DateTime created)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);
            new DirectoryInfo(fullPath).LastWriteTimeUtc = created;
        }
    }
}
