using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Syncr.FileSystems.AmazonS3
{
    [DebuggerDisplay("Directory {RelativePath} Generated? {IsGenerated}")]
    public class S3DirectoryEntry : DirectoryEntry
    {
        public string ObjectKey { get; set; }
        public string BucketName { get; set; }

        public bool IsGenerated
        {
            get { return string.IsNullOrEmpty(this.ObjectKey); }
        }

        public S3DirectoryEntry()
        {
        }

        public override bool CanWriteCreationTime
        {
            get { return false; }
        }

        public override bool CanWriteModificationTime
        {
            get { return false; }
        }


        private static S3DirectoryEntryComparer _defaultComparer = new S3DirectoryEntryComparer();

        public static IEqualityComparer<S3DirectoryEntry> DefaultComparer
        {
            get { return _defaultComparer; }
        }

        internal class S3DirectoryEntryComparer : IEqualityComparer<S3DirectoryEntry>
        {        
            public bool Equals(S3DirectoryEntry x, S3DirectoryEntry y)
            {
                if (x.RelativePath == null || y.RelativePath == null)
                    return false;

                return (x.RelativePath.ToLowerInvariant() == y.RelativePath.ToLowerInvariant());
            }

            public int GetHashCode(S3DirectoryEntry obj)
            {
                return obj.RelativePath.ToLowerInvariant().GetHashCode();
            }
        }
    }
}
