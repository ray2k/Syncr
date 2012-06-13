using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.S3.Model;

namespace Syncr.FileSystems.AmazonS3
{
    [DebuggerDisplay("File {RelativePath}")]
    public class S3FileEntry : FileEntry
    {
        protected Amazon.S3.AmazonS3 S3 { get; private set; }

        public string ObjectKey { get; set; }
        public string BucketName { get; set; }

        public S3FileEntry(Amazon.S3.AmazonS3 s3Client)
        {
            this.S3 = s3Client;
        }

        public override Stream Open()
        {
            var response = this.S3.GetObject(new GetObjectRequest()
                                            .WithKey(this.ObjectKey)
                                            .WithBucketName(this.BucketName));
            return response.ResponseStream;                                            
        }

        public override bool CanWriteCreationTime
        {
            get { return false; }
        }

        public override bool CanWriteModificationTime
        {
            get { return false; }
        }
    }
}
