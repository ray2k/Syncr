using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemWrapper.IO;

namespace Syncr.FileSystems.Native.IO
{
    public class FileSystem : IFileSystem
    {
        public IFileWrap File
        {
            get { return new FileWrap(); }
        }

        public IDirectoryWrap Directory
        {
            get { return new DirectoryWrap(); }
        }

        public IFileInfoWrap GetFileInfo(string filePath)
        {
            return new FileInfoWrap(filePath);
        }

        public IDirectoryInfoWrap GetDirectoryInfo(string directoryPath)
        {
            return new DirectoryInfoWrap(directoryPath);
        }
    }
}