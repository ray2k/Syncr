using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncr.FileSystems.AmazonS3;

namespace Syncr.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var prov = new AmazonS3SyncProvider(
                new AmazonS3FileSystemOptions()
                {
                    AccessKey = "",
                    SecretKey = "",
                    BucketName = ""
                }
            );
            
            var items = prov.GetFileSystemEntries(SearchOption.AllDirectories);
            foreach (var item in items)
            {
                Console.WriteLine("{0} {1} ({2})", 
                    item is S3FileEntry ? "FILE" : "DIRECTORY", 
                    item.RelativePath, item is S3FileEntry ? string.Empty : (item as S3DirectoryEntry).IsGenerated.ToString()
                    );
            }
        }
    }
}
