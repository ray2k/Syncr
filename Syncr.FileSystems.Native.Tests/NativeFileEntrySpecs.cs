using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Shouldly;
using Bddify;
using SystemWrapper.IO;
using SystemWrapper;

namespace Syncr.FileSystems.Native.Tests
{
    public abstract class NativeFileEntrySpec
    {
        public Mock<IFileInfoWrap> MockFileInfo { get; set; }

        public NativeFileEntrySpec()
        {
            this.MockFileInfo = new Mock<IFileInfoWrap>();
        }

        protected NativeFileEntry CurrentInstance { get; set; }
    }

    public class Opening_A_Native_File : NativeFileEntrySpec
    {
        Stream ActualStream;

        public void Context()
        {
            var mockStreamWrap = new Mock<IFileStreamWrap>();
            mockStreamWrap.SetupGet(p => p.StreamInstance).Returns(new MemoryStream());

            MockFileInfo.Setup(p => p.Open(FileMode.Open, FileAccess.Read, FileShare.Read)).Returns(mockStreamWrap.Object);
        }

        public void Given_a_native_FileEntry()
        {
            this.CurrentInstance = new NativeFileEntry(this.MockFileInfo.Object)
            {
                RelativePath = "foobar.txt",
                Name = "foobar.txt",
                BaseDirectory = "c:\\"
            };
        }

        public void When_is_is_opened()
        {
            ActualStream = CurrentInstance.Open();
        }

        public void Then_it_should_stream_its_contents()
        {
            ActualStream.ShouldNotBe(null);
            ActualStream.ShouldBeTypeOf<MemoryStream>();
            MockFileInfo.VerifyAll();
            MockFileInfo.Verify();
        }

        [Fact]
        public void Will_Stream_Its_Contents()
        {
            this.Bddify();
        }
    }

    public class Writing_A_Native_Files_Creation_Time : NativeFileEntrySpec
    {
        public void Context()
        {
            MockFileInfo.SetupSet(p => p.CreationTimeUtc = It.Is<DateTimeWrap>(w => w.DateTimeInstance == DateTime.Now.Date));
        }

        public void Given_a_native_FileEntry()
        {
            CurrentInstance = new NativeFileEntry(MockFileInfo.Object)
            {
                RelativePath = "foobar.txt",
                Name = "foobar.txt",
                BaseDirectory = "c:\\"
            };
        }

        public void When_CreationTimeUtc_is_written()
        {
            CurrentInstance.SetCreationTime(DateTime.Now.Date);
        }

        public void Then_filesystem_creationtimeutc_should_be_updated()
        {
            MockFileInfo.VerifyAll();
            MockFileInfo.VerifyAll();
        }

        [Fact]
        public void Should_update_CreationTimeUtc_on_native_filesystem()
        {
            this.Bddify();
        }
    }

    public class Writing_A_Native_Files_Modification_Time : NativeFileEntrySpec
    {
        public void Context()
        {
            MockFileInfo.SetupSet(p => p.LastWriteTimeUtc = It.Is<DateTimeWrap>(w => w.DateTimeInstance == DateTime.Now.Date));
        }

        public void Given_a_native_FileEntry()
        {
            CurrentInstance = new NativeFileEntry(MockFileInfo.Object)
            {
                RelativePath = "foobar.txt",
                Name = "foobar.txt",
                BaseDirectory = "c:\\"
            };
        }

        public void When_ModificationTime_is_written()
        {
            CurrentInstance.SetModificationTime(DateTime.Now.Date);
        }

        public void Then_filesystem_LastWriteTimeUtc_should_be_updated()
        {
            MockFileInfo.VerifyAll();
        }

        [Fact]
        public void Should_update_CreationTimeUtc_on_native_filesystem()
        {
            this.Bddify();
        }
    }
}
