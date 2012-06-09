using System;
using System.IO;
using SystemWrapper;
using SystemWrapper.IO;

namespace Syncr.FileSystems.Native
{
    public sealed class NativeFileEntry : FileEntry
    {
        private IFileInfoWrap FileInfo { get; set; }
        
        public string BaseDirectory { get; set; }

        public NativeFileEntry(IFileInfoWrap fileInfo)
        {
            this.FileInfo = fileInfo;
        }       

        public override System.IO.Stream Open()
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);            
            return this.FileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read).StreamInstance;
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
            this.FileInfo.CreationTimeUtc = new DateTimeWrap(created);
        }

        protected override void WriteModificationTime(DateTime modified)
        {
            string fullPath = Path.Combine(this.BaseDirectory, this.RelativePath);
            this.FileInfo.LastWriteTimeUtc = new DateTimeWrap(modified);
        }
    }
}
