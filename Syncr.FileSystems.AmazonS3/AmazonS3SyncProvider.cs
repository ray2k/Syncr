using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.S3.Model;
using Amazon.Util;
using Syncr;

namespace Syncr.FileSystems.AmazonS3
{
    public class AmazonS3SyncProvider : SyncProviderBase
    {
        protected Amazon.S3.AmazonS3 S3 { get; private set; }
        protected AmazonS3FileSystemOptions Options { get; private set; }

        private static Amazon.S3.AmazonS3Client CreateClient(AmazonS3FileSystemOptions options)
        {
            return new Amazon.S3.AmazonS3Client(options.AccessKey, options.SecretKey);
        }

        public AmazonS3SyncProvider(AmazonS3FileSystemOptions options)
            : this(CreateClient(options), options)
        {
        }

        public AmazonS3SyncProvider(Amazon.S3.AmazonS3 s3Client, AmazonS3FileSystemOptions options)
        {
            this.S3 = s3Client;
            this.Options = options;
        }

        public override IEnumerable<FileSystemEntry> GetFileSystemEntries(SearchOption searchOption)
        {
            List<FileSystemEntry> results = new List<FileSystemEntry>();

            var listObjectsResponse = S3.ListObjects(
                new ListObjectsRequest()
                .WithBucketName(this.Options.BucketName)
            );
            
            while (true)
            {
                foreach (var found in listObjectsResponse.S3Objects)
                {
                    results.AddRange(BuildS3Entries(found, searchOption));
                }

                if (listObjectsResponse.NextMarker == null)
                    break;
                else
                    listObjectsResponse = S3.ListObjects(
                        new ListObjectsRequest()
                        .WithBucketName(this.Options.BucketName)
                        .WithMarker(listObjectsResponse.NextMarker)
                    );
            }

            // remove duplicate generated entries if any
            var directoryEntries = results.OfType<S3DirectoryEntry>();
            var distinctDirectoryEntries = new HashSet<S3DirectoryEntry>(directoryEntries, S3DirectoryEntry.DefaultComparer);

            return distinctDirectoryEntries.Cast<FileSystemEntry>().Concat(results.OfType<S3FileEntry>().Cast<FileSystemEntry>());
        }

        private IEnumerable<FileSystemEntry> BuildS3Entries(S3Object found, SearchOption searchOption)
        {
            // we have a single S3 item
            // it can be 
            //      a single file "foo.bin"
            //      a single file under a subfolder "subfolder/foo.bin"
            //      a subfolder "subfolder/", "subfolder1/subfolder2/"
            List<FileSystemEntry> results = new List<FileSystemEntry>();

            DateTime modified = DateTime.ParseExact(found.LastModified, AWSSDKUtils.GMTDateFormat, CultureInfo.CurrentCulture);

            bool isFileOnRoot = (found.Key.Contains("/") == false);
            bool isFileBeyondRoot = (found.Key.Contains("/") == true && found.Key.EndsWith("/") == false);
            bool isFolderOnRoot = (found.Key.IndexOf("/") == (found.Key.Length - 1));
            bool isFolderBeyondRoot = (found.Key.EndsWith("/") && (found.Key.IndexOf("/") != (found.Key.Length - 1)));

            string pathAdjustedKey = found.Key.Replace('/', Path.DirectorySeparatorChar);

            List<S3DirectoryEntry> generatedSubDirectories = new List<S3DirectoryEntry>();

            if (isFileOnRoot || (isFileBeyondRoot == true && searchOption == SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(found.Key);
                
                results.Add(
                    new S3FileEntry(this.S3)
                    {
                        BucketName = this.Options.BucketName,
                        Created = modified,
                        Modified = modified,
                        ObjectKey = found.Key,
                        RelativePath = pathAdjustedKey,
                        Size = found.Size,
                        Name = fileName
                    }
                );

                if (isFileBeyondRoot)
                {
                    // stash any subdirectories
                    foreach (var generated in ParseSubdirectories(pathAdjustedKey, Path.DirectorySeparatorChar))
                    {
                        generated.Created = modified;
                        generated.Modified = modified;
                        generated.BucketName = this.Options.BucketName;
                        results.Add(generated);
                    }
                }
            }           

            if (isFolderOnRoot || (isFolderBeyondRoot && searchOption == SearchOption.AllDirectories))
            {
                string folderName = null;
                string adjustedKeyWithoutTrailing = pathAdjustedKey.WithoutTrailingPathSeparator();

                if (isFolderOnRoot)
                    folderName = adjustedKeyWithoutTrailing;
                if (isFolderBeyondRoot)
                    folderName = adjustedKeyWithoutTrailing.Substring(adjustedKeyWithoutTrailing.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                results.Add(
                    new S3DirectoryEntry()
                    {                        
                        BucketName = this.Options.BucketName,
                        ObjectKey = found.Key,
                        Created = modified,
                        Modified = modified,
                        Name = folderName,
                        RelativePath = adjustedKeyWithoutTrailing                                    
                    }
                );

                if (isFolderBeyondRoot)
                {
                    // stash any subdirectories
                    foreach (var generated in ParseSubdirectories(pathAdjustedKey, Path.DirectorySeparatorChar))
                    {
                        generated.Created = modified;
                        generated.Modified = modified;
                        generated.BucketName = this.Options.BucketName;
                        results.Add(generated);
                    }
                }
            }

            return generatedSubDirectories.Cast<FileSystemEntry>().Concat(results);
        }

        public override FileSystemEntry Create(FileSystemEntry entry)
        {
            if (entry is FileEntry)
                return CreateS3File(entry as FileEntry);
            if (entry is DirectoryEntry)
                return CreateS3Directory(entry as DirectoryEntry);

            return null;
        }

        private S3FileEntry CreateS3File(FileEntry fileEntry)
        {
            var request = (PutObjectRequest) (new PutObjectRequest()
                .WithKey(fileEntry.RelativePath.Replace(Path.DirectorySeparatorChar, '/'))
                .WithInputStream(fileEntry.Open()));

            var putObjectResponse = S3.PutObject( (PutObjectRequest) request);

            return new S3FileEntry(this.S3)
            {
                BucketName = this.Options.BucketName,
                RelativePath = fileEntry.RelativePath,
                ObjectKey = request.Key,
                Size = fileEntry.Size,
                Name = Path.GetFileName(fileEntry.RelativePath)
            };                           
        }

        private S3DirectoryEntry CreateS3Directory(DirectoryEntry directoryEntry)
        {
            var request = new PutObjectRequest()
                .WithKey(directoryEntry.RelativePath.WithTrailingPathSeparator()
                         .Replace(Path.DirectorySeparatorChar, '/'));

            var putObjectResponse = S3.PutObject((PutObjectRequest)request);

            return new S3DirectoryEntry()
            {
                BucketName = this.Options.BucketName,
                RelativePath = directoryEntry.RelativePath,
                ObjectKey = request.Key,
                Name = Path.GetFileName(directoryEntry.RelativePath)
            };      
        }

        public override void Delete(FileSystemEntry entry)
        {
            if (entry is FileEntry)
                DeleteS3File(entry as FileEntry);
            if (entry is DirectoryEntry)
                DeleteS3Directory(entry as DirectoryEntry);
        }

        private void DeleteS3Directory(DirectoryEntry directoryEntry)
        {
            this.S3.DeleteObject(
                new DeleteObjectRequest()
                .WithKey(directoryEntry.RelativePath
                         .WithTrailingPathSeparator()
                         .Replace(Path.DirectorySeparatorChar, '/')
                ).WithBucketName(this.Options.BucketName));
        }

        private void DeleteS3File(FileEntry fileEntry)
        {
            this.S3.DeleteObject(
                new DeleteObjectRequest().WithKey(fileEntry.RelativePath.Replace(Path.DirectorySeparatorChar, '/'))
                .WithBucketName(this.Options.BucketName));
        }

        private IEnumerable<S3DirectoryEntry> ParseSubdirectories(string objectPath, char separator)
        {
            var pieces = objectPath.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            if (objectPath.EndsWith(separator.ToString()) == false) // is a file so trailing file name entry
                pieces = pieces.Take(pieces.Length - 1).ToArray();

            List<S3DirectoryEntry> result = new List<S3DirectoryEntry>();

            for (int i = 0; i < pieces.Length; i++)
            {
                if (i == 0)
                {
                    result.Add(new S3DirectoryEntry() { Name = pieces[0], RelativePath = pieces[0] });
                }
                else
                {
                    var targetPieces = pieces.Take(i + 1).ToArray();

                    result.Add(
                        new S3DirectoryEntry()
                        {
                            Name = pieces[i],
                            RelativePath = string.Join(separator.ToString(), targetPieces)
                        }
                    );
                }
            }

            return result;
        }
    }
}
