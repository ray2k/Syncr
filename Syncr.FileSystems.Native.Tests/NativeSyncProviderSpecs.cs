using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Shouldly;
using Bddify;
using System.IO;

namespace Syncr.FileSystems.Native.Tests
{
    public abstract class NativeSyncProviderSpec
    {
        public Mock<IFileSystem> MockFileSystem;

        public NativeSyncProviderSpec()
        {
            MockFileSystem = new Mock<IFileSystem>();
        }

        protected NativeSyncProvider CreateInstance(LinuxFileSystemOptions options)
        {
            return new NativeSyncProvider(MockFileSystem.Object, options);
        }

        protected NativeSyncProvider CreateInstance(WindowsFileSystemOptions options)
        {
            return new NativeSyncProvider(MockFileSystem.Object, options);
        }

        protected NativeSyncProvider CurrentInstance { get; set; }
    }

    // TODO: creating a directory
    // deleting a directory
    // deleting a file

    public class Creating_A_Native_File : NativeSyncProviderSpec
    {
        FakeFileEntry FakeFile;
        byte[] fakeContents = Guid.NewGuid().ToByteArray();
        FileSystemEntry CreatedFile;
        MemoryStream CreatedStream = new MemoryStream();

        Mock<FileBase> MockFile = new Mock<FileBase>();
        Mock<IFileInfoFactory> MockFileInfoFactory = new Mock<IFileInfoFactory>();
        Mock<FileInfoBase> MockFileInfo = new Mock<FileInfoBase>();

        public void Context()
        {
            MockFileSystem.SetupGet(p => p.File).Returns(MockFile.Object);
            MockFileSystem.SetupGet(p => p.FileInfo).Returns(MockFileInfoFactory.Object);

            MockFile.Setup(p => p.Exists("c:\\somefile.txt")).Returns(false);
            MockFile.Setup(p => p.Create("c:\\somefile.txt")).Returns(CreatedStream);
                       
            MockFileInfo.SetupGet(p => p.CreationTimeUtc).Returns(DateTime.Now.Date);
            MockFileInfo.SetupGet(p => p.LastWriteTimeUtc).Returns(DateTime.Now.Date);

            MockFileInfoFactory.Setup(p => p.FromFileName("c:\\somefile.txt")).Returns(MockFileInfo.Object);
        }

        public void Given_a_file_to_be_written()
        {
            fakeContents = Guid.NewGuid().ToByteArray();
            FakeFile = new FakeFileEntry(new MemoryStream(fakeContents) { Position = 0 })
            {
                RelativePath = "somefile.txt"
            };
        }

        public void When_it_is_created_on_native_filesystem()
        {
            CurrentInstance = CreateInstance(new WindowsFileSystemOptions() { Path = "c:\\" });
            CreatedFile = CurrentInstance.Create(FakeFile);
        }

        public void Then_the_file_contents_are_written_to_the_native_filesystem()
        {
            MockFile.VerifyAll();
            MockFileInfo.VerifyAll();
            MockFileInfoFactory.VerifyAll();
            MockFileSystem.VerifyAll();            
        }

        [Fact]
        public void Writes_the_file_to_native_filesystem()
        {
            this.Bddify();
        }
    }

    public class Querying_For_Native_Files : NativeSyncProviderSpec
    {
        IList<FileSystemEntry> FoundEntries;

        public void Context()
        {
            var mockDirectory = new Mock<DirectoryBase>();
            var mockFileInfoFactory = new Mock<IFileInfoFactory>();
            MockFileSystem.SetupGet(p => p.FileInfo).Returns(mockFileInfoFactory.Object);
            MockFileSystem.Setup(p => p.Directory).Returns(mockDirectory.Object);
            
            var fakeFileInfo = new Mock<FileInfoBase>();
            fakeFileInfo.SetupGet(p => p.CreationTimeUtc).Returns(DateTime.Now.Date);
            fakeFileInfo.SetupGet(p =>p.LastWriteTimeUtc).Returns(DateTime.Now.Date);
            fakeFileInfo.SetupGet(p => p.Length).Returns(100);
            fakeFileInfo.SetupGet(p => p.FullName).Returns("c:\\somefile.txt");
            mockFileInfoFactory.Setup(p => p.FromFileName("c:\\somefile.txt")).Returns(fakeFileInfo.Object);

            var fakeDirInfo = new Mock<FileInfoBase>();
            fakeFileInfo.SetupGet(p => p.CreationTimeUtc).Returns(DateTime.Now.Date);
            fakeFileInfo.SetupGet(p =>p.LastWriteTimeUtc).Returns(DateTime.Now.Date);
            fakeDirInfo.SetupGet(p => p.Name).Returns("somedir");
            fakeDirInfo.SetupGet(p => p.FullName).Returns("c:\\somedir");
            mockFileInfoFactory.Setup(p => p.FromFileName("c:\\somedir")).Returns(fakeDirInfo.Object);

            mockDirectory.Setup(p => p.GetFiles("c:\\", "*", System.IO.SearchOption.AllDirectories)).Returns(
                new string[] { "c:\\somefile.txt" });

            mockDirectory.Setup(p => p.GetDirectories("c:\\", "*", System.IO.SearchOption.AllDirectories)).Returns(
                new string[] { "c:\\somedir" });
        }

        public void Given_a_directory_with_a_file_and_subdirectory()
        {
            var options = new WindowsFileSystemOptions()
            {
                Path = "c:\\"
            };

            this.CurrentInstance = CreateInstance(options);
        }

        public void When_it_is_searched_for_filesystem_entries()
        {
            FoundEntries = CurrentInstance.GetFileSystemEntries(System.IO.SearchOption.AllDirectories).ToList();
        }

        public void Then_an_entry_for_each_file_and_subdirectory_should_be_returned()
        {
            FoundEntries.ShouldNotBe(null);
            FoundEntries.Count.ShouldBe(2);
            FoundEntries.OfType<NativeFileEntry>().Count().ShouldBe(1);
            FoundEntries.OfType<NativeDirectoryEntry>().Count().ShouldBe(1);
        }

        [Fact]
        public void Should_return_NativeFileEntry_for_each_file_and_NativeDirectoryEntry_for_each_subdirectory()
        {
            this.Bddify();
        }
    }
}
