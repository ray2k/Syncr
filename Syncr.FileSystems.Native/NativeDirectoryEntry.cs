using System;
using System.IO;
using SystemWrapper;
using SystemWrapper.IO;

namespace Syncr.FileSystems.Native
{
    public sealed class NativeDirectoryEntry : DirectoryEntry
    {
        public string BaseDirectory { get; set; }

        private IDirectoryInfoWrap DirectoryInfo { get; set; }

        public NativeDirectoryEntry(IDirectoryInfoWrap directoryInfo)
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
            this.DirectoryInfo.CreationTimeUtc = new DateTimeWrap(createdUtc);
        }

        protected override void WriteModificationTime(DateTime modifiedUtc)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);
            this.DirectoryInfo.LastWriteTimeUtc = new DateTimeWrap(modifiedUtc);
        }
    }
}
