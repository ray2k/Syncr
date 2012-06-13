using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Syncr.FileSystems.AmazonS3
{
    public class AmazonS3FileSystemOptions
    {
        // async options?
        [Required]
        [Description("Name of the S3 Bucket")]
        public string BucketName { get; set; }

        [Required]
        [Description("AWS Access Key")]
        public string AccessKey { get; set; }
        
        [Required]
        [Description("AWS Secret Key")]
        public string SecretKey { get; set; }
    }
}
