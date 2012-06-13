using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Bddify;
using Amazon.S3.Model;
using Amazon.Util;
using System.IO;
using Shouldly;
using Syncr.FileSystems.Native.Tests;

namespace Syncr.FileSystems.AmazonS3.Tests
{
    public abstract class AmazonS3SyncProviderSpec
    {
        public Mock<Amazon.S3.AmazonS3> MockS3;

        public AmazonS3SyncProviderSpec()
        {
            MockS3 = new Mock<Amazon.S3.AmazonS3>();
            CurrentInstance = CreateInstance();
        }

        protected AmazonS3SyncProvider CreateInstance()
        {
            return new AmazonS3SyncProvider(MockS3.Object,
                new AmazonS3FileSystemOptions()
                {
                    BucketName = "BucketName"
                }
            );
        }

        protected AmazonS3SyncProvider CurrentInstance { get; set; }
    }

    public class Querying_For_S3_Files : AmazonS3SyncProviderSpec
    {
        List<FileSystemEntry> FoundEntries;

        public void Given_an_s3_bucket_with_some_files_and_folders()
        {
            List<S3Object> fakeObjects = new List<S3Object>()
            {
                new S3Object()
                {
                    Key = "subfolder/File.bin",
                    Size = 100,
                    LastModified = AWSSDKUtils.FormattedCurrentTimestampISO8601,
                    BucketName = "BucketName"
                },
                new S3Object()
                {
                    Key = "subfolder2/subfolder3/",
                    LastModified = AWSSDKUtils.FormattedCurrentTimestampISO8601,
                    BucketName = "BucketName"
                }
            };
            var fakeResponse = new ListObjectsResponse();
            fakeResponse.S3Objects.AddRange(fakeObjects);

            MockS3.Setup(p => p.ListObjects(
                It.Is<ListObjectsRequest>(req => req.BucketName == "BucketName")
                )).Returns(fakeResponse);
        }

        public void When_it_is_searched_for_filesystem_entries()
        {
            FoundEntries = CurrentInstance.GetFileSystemEntries(SearchOption.AllDirectories).ToList();
        }

        public void Then_an_entry_for_each_file_and_subdirectory_should_be_returned()
        {
            FoundEntries.ShouldNotBe(null);
            FoundEntries.Count.ShouldBe(4);
            FoundEntries.OfType<S3FileEntry>().Count().ShouldBe(1);
            FoundEntries.OfType<S3DirectoryEntry>().Count().ShouldBe(3);
            FoundEntries.OfType<S3DirectoryEntry>().Where(p => p.IsGenerated == true).Count().ShouldBe(2);
            MockS3.VerifyAll();
        }

        [Fact]
        public void Should_return_an_entry_for_each_file_and_subdirectory_should_be_returned()
        {
            this.Bddify();
        }
    }

    public class Creating_An_S3_File : AmazonS3SyncProviderSpec
    {
        FileSystemEntry CreatedEntry;

        public void Given_an_s3_bucket()
        {            
        }

        public void When_a_file_is_created_in_the_bucket()
        {
            var ms = new MemoryStream(Guid.NewGuid().ToByteArray());
            ms.Position = 0;

            CreatedEntry = CurrentInstance.Create(
                new FakeFileEntry(ms)
                {
                    Size = 100,
                    Created = DateTime.Now.Date,
                    Modified = DateTime.Now.Date,
                    Name = "File.bin",
                    RelativePath = "File.bin"
                }
            );
        }

        public void Then_the_s3_object_should_be_created()
        {
            CreatedEntry.ShouldNotBe(null);
            CreatedEntry.ShouldBeTypeOf<S3FileEntry>();
            var typedEntry = CreatedEntry as S3FileEntry;
            typedEntry.RelativePath.ShouldBe("File.bin");
            typedEntry.ObjectKey.ShouldBe("File.bin");
            typedEntry.Name.ShouldBe("File.bin");
            typedEntry.BucketName.ShouldBe("BucketName");
            MockS3.VerifyAll();
        }

        [Fact]
        public void Should_create_the_file_on_s3_and_return_the_corresponding_entry()
        {
            this.Bddify();
        }
    }

    public class Creating_An_S3_Directory : AmazonS3SyncProviderSpec
    {
        FileSystemEntry CreatedEntry;

        public void Given_an_s3_bucket()
        {
        }

        public void When_a_directory_is_created_in_the_bucket()
        {
            CreatedEntry = CurrentInstance.Create(
                new FakeDirectoryEntry()
                {
                    Created = DateTime.Now.Date,
                    Modified = DateTime.Now.Date,
                    Name = "subfolder",
                    RelativePath = "subfolder"
                }
            );
        }

        public void Then_the_s3_object_should_be_created()
        {
            CreatedEntry.ShouldNotBe(null);
            CreatedEntry.ShouldBeTypeOf<S3DirectoryEntry>();
            var typedEntry = CreatedEntry as S3DirectoryEntry;
            typedEntry.RelativePath.ShouldBe("subfolder");
            typedEntry.ObjectKey.ShouldBe("subfolder/");
            typedEntry.Name.ShouldBe("subfolder");
            typedEntry.BucketName.ShouldBe("BucketName");
            MockS3.VerifyAll();
        }

        [Fact]
        public void Should_create_the_file_on_s3_and_return_the_corresponding_entry()
        {
            this.Bddify();
        }
    }

    public class Deleting_A_S3_File : AmazonS3SyncProviderSpec
    {
        public void Given_an_s3_filesystem()
        {
            MockS3.Setup(p => p.DeleteObject(
                It.Is<DeleteObjectRequest>(req =>
                    req.BucketName == "BucketName" &&
                    req.Key == "subfolder/file.txt")));
        }

        public void When_a_file_is_deleted()
        {
            CurrentInstance.Delete(
                new FakeFileEntry()
                {
                    RelativePath = "subfolder\\file.txt"
                }
            );
        }

        public void Then_the_s3_object_should_be_deleted_from_the_bucket()
        {
            MockS3.VerifyAll();
        }

        [Fact]
        public void Should_delete_the_s3_file_object_from_the_bucket()
        {
            this.Bddify();
        }
    }

    public class Deleting_A_S3_Directory : AmazonS3SyncProviderSpec
    {
        public void Given_an_s3_filesystem()
        {
            MockS3.Setup(p => p.DeleteObject(
                It.Is<DeleteObjectRequest>(req =>
                    req.BucketName == "BucketName" &&
                    req.Key == "subfolder/")));
        }

        public void When_a_directory_is_deleted()
        {
            CurrentInstance.Delete(
                new FakeDirectoryEntry()
                {
                    RelativePath = "subfolder"
                }
            );
        }

        public void Then_the_s3_object_should_be_deleted_from_the_bucket()
        {
            MockS3.VerifyAll();
        }

        [Fact]
        public void Should_delete_the_s3_directory_object_from_the_bucket()
        {
            this.Bddify();
        }
    }
}
