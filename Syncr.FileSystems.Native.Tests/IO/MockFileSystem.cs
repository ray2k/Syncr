using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Syncr.FileSystems.Native.IO;
using SystemWrapper.IO;

namespace Syncr.FileSystems.Native.Tests.IO
{
    public class MockFileSystem : IFileSystem
    {
        private Dictionary<string, IFileInfoWrap> _fileInfos = new Dictionary<string, IFileInfoWrap>();
        private Dictionary<string, IDirectoryInfoWrap> _directoryInfos = new Dictionary<string, IDirectoryInfoWrap>();

        public MockFileSystem()
        {
            MockFile = new Mock<IFileWrap>();
            MockDirectory = new Mock<IDirectoryWrap>();
        }

        public Mock<IFileWrap> MockFile { get; private set; }
        
        public IFileWrap File
        {
            get { return MockFile.Object; }
        }

        public Mock<IDirectoryWrap> MockDirectory { get; private set; }

        public IDirectoryWrap Directory
        {
            get { return MockDirectory.Object; }
        }

        public void AddFileInfo(string filePath, IFileInfoWrap fileInfo)
        {
            _fileInfos.Remove(filePath.ToLowerInvariant());
            _fileInfos.Add(filePath.ToLowerInvariant(), fileInfo);
        }

        public void AddDirectoryInfo(string directoryPath, IDirectoryInfoWrap directoryInfo)
        {
            _directoryInfos.Remove(directoryPath.ToLowerInvariant());
            _directoryInfos.Add(directoryPath.ToLowerInvariant(), directoryInfo);
        }

        public IFileInfoWrap GetFileInfo(string filePath)
        {
            IFileInfoWrap result = null;

            _fileInfos.TryGetValue(filePath.ToLowerInvariant(), out result);

            return result;
        }

        public IDirectoryInfoWrap GetDirectoryInfo(string directoryPath)
        {
            IDirectoryInfoWrap result = null;

            _directoryInfos.TryGetValue(directoryPath.ToLower(), out result);

            return result;
        }

        public void VerifyAll()
        {
            this.MockDirectory.VerifyAll();
            this.MockFile.VerifyAll();
        }
    }
}
