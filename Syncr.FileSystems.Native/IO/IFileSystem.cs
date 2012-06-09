using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemWrapper.IO;

namespace Syncr.FileSystems.Native.IO
{
    public interface IFileSystem
    {
        IFileWrap File { get; }
        IDirectoryWrap Directory { get; }
        IFileInfoWrap GetFileInfo(string filePath);
        IDirectoryInfoWrap GetDirectoryInfo(string directoryPath);
    }
}
