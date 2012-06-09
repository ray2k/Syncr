using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Shouldly;
using Bddify;
using System.IO;
using Syncr.FileSystems.Native.IO;
using Syncr.FileSystems.Native.Tests.IO;
using SystemWrapper.IO;
using SystemWrapper;
using System.Net;
using System.Runtime.InteropServices;

namespace Syncr.FileSystems.Native.Tests
{
    public abstract class NativeSyncProviderSpec
    {
        public MockFileSystem MockFileSystem;
        public Mock<INetworkConnectionFactory> MockConnectionFactory;

        public NativeSyncProviderSpec()
        {
            MockFileSystem = new MockFileSystem();
            MockConnectionFactory = new Mock<INetworkConnectionFactory>();
        }

        protected NativeSyncProvider CreateInstance(LinuxFileSystemOptions options)
        {
            return new NativeSyncProvider(MockFileSystem, options);
        }

        protected NativeSyncProvider CreateInstance(WindowsFileSystemOptions options)
        {
            return new NativeSyncProvider(MockFileSystem, MockConnectionFactory.Object, options);
        }

        protected NativeSyncProvider CurrentInstance { get; set; }
    }   

    public class Querying_Contents_Using_Network_Path : NativeSyncProviderSpec
    {
        Mock<INetworkConnection> MockConnection;
        IList<FileSystemEntry> FoundEntires;

        public void Context()
        {
            MockConnection = new Mock<INetworkConnection>();
            MockConnection.Setup(p => p.Connect());

            MockConnectionFactory.Setup(p => p.CreateConnection("\\\\remotepath\\remotedir", "username", "password")).Returns(MockConnection.Object);
        }

        public void Given_a_remote_native_filesystem_with_a_file_and_directory()
        {
            CurrentInstance = CreateInstance(
                new WindowsFileSystemOptions()
                {
                    Path = "\\\\remotepath\\remotedir",
                    Password = "password",
                    UserName = "username"
                }
            );
        }

        public void When_it_is_queried_for_contents()
        {
            FoundEntires = CurrentInstance.GetFileSystemEntries(SearchOption.AllDirectories).ToList();
        }

        public void Then_it_should_make_a_connection_to_the_remote_filesystem()
        {
            MockConnectionFactory.VerifyAll();
            MockConnection.VerifyAll();
        }

        [Fact]
        public void Should_connect_to_the_remote_filsystem_over_the_network()
        {
            this.Bddify();
        }
    }


    public class Deleting_A_Native_Directory : NativeSyncProviderSpec
    {
        public void Context()
        {
            MockFileSystem.MockDirectory.Setup(p => p.Exists("c:\\somedir")).Returns(true);
            MockFileSystem.MockDirectory.Setup(p => p.Delete("c:\\somedir"));
        }

        public void Given_a_directory_to_be_deleted()
        {
            CurrentInstance = CreateInstance(new WindowsFileSystemOptions() { Path = "c:\\" });
        }

        public void When_the_directory_is_deleted_from_native_filesystem()
        {
            CurrentInstance.Delete(new FakeDirectoryEntry() { RelativePath = "somedir" });
        }

        public void Then_it_is_removed_from_the_filesystem()
        {
            MockFileSystem.VerifyAll();
        }

        [Fact]
        public void Deletes_the_native_Directory()
        {
            this.Bddify();
        }
    }

    public class Creating_A_Native_Directory : NativeSyncProviderSpec
    {
        protected Mock<IDirectoryInfoWrap> MockDirectoryInfo;
        protected FileSystemEntry CreatedEntry;

        public void Context()
        {
            MockDirectoryInfo = new Mock<IDirectoryInfoWrap>();
            MockDirectoryInfo.SetupGet(p => p.CreationTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockDirectoryInfo.SetupGet(p => p.LastWriteTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockFileSystem.AddDirectoryInfo("c:\\somedir", MockDirectoryInfo.Object);

            MockFileSystem.MockDirectory.Setup(p => p.Exists("c:\\somedir")).Returns(false);
            MockFileSystem.MockDirectory.Setup(p => p.CreateDirectory("c:\\somedir")).Returns(new Mock<IDirectoryInfoWrap>().Object);
        }

        public void Given_a_directory_to_be_created()
        {
            CurrentInstance = CreateInstance(new WindowsFileSystemOptions() { Path = "c:\\" });
        }

        public void When_it_is_created_on_native_filesystem()
        {
            CreatedEntry = CurrentInstance.Create(
                new FakeDirectoryEntry() { RelativePath = "somedir" });
        }

        public void Then_the_directory_is_created_on_the_native_filesystem()
        {
            MockDirectoryInfo.VerifyAll();
            MockFileSystem.VerifyAll();
        }

        [Fact]
        public void Creates_the_directory_on_the_native_filesystem()
        {
            this.Bddify();
        }
    }

    public class Deleting_A_Native_File : NativeSyncProviderSpec
    {
        public void Context()
        {
            MockFileSystem.MockFile.Setup(p => p.Exists("c:\\somefile.txt")).Returns(true);
            MockFileSystem.MockFile.Setup(p => p.Delete("c:\\somefile.txt"));
        }

        public void Given_a_directory_to_be_deleted()
        {
            CurrentInstance = CreateInstance(new WindowsFileSystemOptions() { Path = "c:\\" });
        }

        public void When_the_directory_is_deleted_from_native_filesystem()
        {
            CurrentInstance.Delete(new FakeFileEntry() { RelativePath = "somefile.txt" });
        }

        public void Then_it_is_removed_from_the_filesystem()
        {
            MockFileSystem.VerifyAll();
        }

        [Fact]
        public void Deletes_the_native_file()
        {
            this.Bddify();
        }
    }

    public class Creating_A_Native_File : NativeSyncProviderSpec
    {
        protected Mock<IFileInfoWrap> MockFileInfo;
        protected FileSystemEntry CreatedEntry;
        protected MemoryStream FakeSourceStream;

        public void Context()
        {
            MockFileInfo = new Mock<IFileInfoWrap>();
            MockFileInfo.SetupGet(p => p.CreationTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockFileInfo.SetupGet(p => p.LastWriteTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockFileSystem.AddFileInfo("c:\\somefile.txt", MockFileInfo.Object);

            MockFileSystem.MockFile.Setup(p => p.Exists("c:\\somefile.txt")).Returns(false);

            var mockStreamWrap = new Mock<IFileStreamWrap>();
            mockStreamWrap.SetupGet(p => p.StreamInstance).Returns(new MemoryStream());

            MockFileSystem.MockFile.Setup(p => p.Create("c:\\somefile.txt")).Returns(mockStreamWrap.Object);
        }

        public void Given_a_file_to_be_created()
        {
            FakeSourceStream = new MemoryStream(Guid.NewGuid().ToByteArray());
            FakeSourceStream.Position = 0;
            
            CurrentInstance = CreateInstance(new WindowsFileSystemOptions() { Path = "c:\\" });
        }

        public void When_it_is_created_on_native_filesystem()
        {
            CreatedEntry = CurrentInstance.Create(
                new FakeFileEntry(new MemoryStream())
                {
                    RelativePath = "somefile.txt"
                }
            );
        }

        public void Then_the_file_is_created_on_the_native_filesystem()
        {
            MockFileInfo.VerifyAll();
            MockFileSystem.VerifyAll();         
        }

        [Fact]
        public void Creates_the_file_on_the_native_filesystem()
        {
            this.Bddify();
        }
    }

    public class Querying_For_Native_Files : NativeSyncProviderSpec
    {
        protected IList<FileSystemEntry> FoundEntries;
        protected Mock<IFileInfoWrap> MockFile = new Mock<IFileInfoWrap>();
        protected Mock<IDirectoryInfoWrap> MockDir = new Mock<IDirectoryInfoWrap>();

        public void Context()
        {
            MockFile.SetupGet(p => p.CreationTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockFile.SetupGet(p => p.LastWriteTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockFile.SetupGet(p => p.Length).Returns(100);
            MockFile.SetupGet(p => p.FullName).Returns("c:\\somefile.txt");
            MockFileSystem.AddFileInfo("c:\\somefile.txt", MockFile.Object);

            MockDir.SetupGet(p => p.CreationTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockDir.SetupGet(p => p.LastWriteTimeUtc).Returns(new DateTimeWrap(DateTime.Now.Date));
            MockDir.SetupGet(p => p.Name).Returns("somedir");
            MockDir.SetupGet(p => p.FullName).Returns("c:\\somedir");
            MockFileSystem.AddDirectoryInfo("c:\\somedir", MockDir.Object);

            MockFileSystem.MockDirectory.Setup(p => p.GetFiles("c:\\", "*", System.IO.SearchOption.AllDirectories)).Returns(
                new string[] { "c:\\somefile.txt" });

            MockFileSystem.MockDirectory.Setup(p => p.GetDirectories("c:\\", "*", System.IO.SearchOption.AllDirectories)).Returns(
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
            MockFileSystem.VerifyAll();
            MockFile.VerifyAll();
            MockDir.VerifyAll();
        }

        [Fact]
        public void Should_return_NativeFileEntry_for_each_file_and_NativeDirectoryEntry_for_each_subdirectory()
        {
            this.Bddify();
        }
    }
}
