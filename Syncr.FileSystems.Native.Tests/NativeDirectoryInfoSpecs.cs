using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Bddify;
using SystemWrapper.IO;
using SystemWrapper;

namespace Syncr.FileSystems.Native.Tests
{
    public abstract class NativeDirectoryInfoSpec
    {
        public Mock<IDirectoryInfoWrap> MockDirectoryInfo { get; set; }

        public NativeDirectoryInfoSpec()
        {
            this.MockDirectoryInfo = new Mock<IDirectoryInfoWrap>();
        }

        protected NativeDirectoryEntry CurrentInstance { get; set; }
    }

    public class Writing_A_Native_Directorys_Creation_Time : NativeDirectoryInfoSpec
    {
        public void Context()
        {
            MockDirectoryInfo.SetupSet(p => p.CreationTimeUtc = It.Is<DateTimeWrap>(w => w.DateTimeInstance == DateTime.Now.Date));
        }

        public void Given_a_native_DirectoryEntry()
        {
            CurrentInstance = new NativeDirectoryEntry(MockDirectoryInfo.Object)
            {
                RelativePath = "somedir",
                Name = "somedir",
                BaseDirectory = "c:\\"
            };
        }

        public void When_CreationTimeUtc_is_written()
        {
            CurrentInstance.SetCreationTime(DateTime.Now.Date);
        }

        public void Then_filesystem_creationtimeutc_should_be_updated()
        {
            MockDirectoryInfo.VerifyAll();
        }

        [Fact]
        public void Should_update_CreationTimeUtc_on_native_filesystem()
        {
            this.Bddify();
        }
    }

    public class Writing_A_Native_Directorys_Modification_Time : NativeDirectoryInfoSpec
    {
        public void Context()
        {
            MockDirectoryInfo.SetupSet(p => p.LastWriteTimeUtc = It.Is<DateTimeWrap>(w => w.DateTimeInstance == DateTime.Now.Date));
        }

        public void Given_a_native_DirectoryEntry()
        {
            CurrentInstance = new NativeDirectoryEntry(MockDirectoryInfo.Object)
            {
                RelativePath = "somedir",
                Name = "somedir",
                BaseDirectory = "c:\\"
            };
        }

        public void When_ModificationTime_is_written()
        {
            CurrentInstance.SetModificationTime(DateTime.Now.Date);
        }

        public void Then_filesystem_LastWriteTimeUtc_should_be_updated()
        {
            MockDirectoryInfo.VerifyAll();
        }

        [Fact]
        public void Should_update_CreationTimeUtc_on_native_filesystem()
        {
            this.Bddify();
        }
    }
}
