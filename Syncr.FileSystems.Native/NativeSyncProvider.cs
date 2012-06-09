using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syncr.FileSystems.Native.IO;

namespace Syncr.FileSystems.Native
{
    public class NativeSyncProvider : ISyncProvider
    {
        protected string BaseDirectory { get; private set; }
        protected string SourcePath { get; private set; }
        protected string UserName { get; private set; }
        protected string Password { get; private set; }

        protected IFileSystem FileSystem { get; private set; }

        public NativeSyncProvider(IFileSystem fileSystem, WindowsFileSystemOptions options)
        {
            this.FileSystem = fileSystem;
            this.SourcePath = options.Path;
            this.UserName = options.UserName;
            this.Password = options.Password;
            this.BaseDirectory = options.Path.WithTrailingPathSeparator();
        }

        public NativeSyncProvider(WindowsFileSystemOptions options)
            : this(new FileSystem(), options)
        {   
        }

        public NativeSyncProvider(LinuxFileSystemOptions options)
            : this(new FileSystem(), options)
        {
        }

        public NativeSyncProvider(IFileSystem fileSystem, LinuxFileSystemOptions options)
        {
            this.FileSystem = fileSystem;
            this.SourcePath = options.Path;
            this.BaseDirectory = options.Path.WithTrailingPathSeparator();
        }

        private string GetRelativePath(string fullPath)
        {
            return fullPath.Substring(0, fullPath.Length - this.BaseDirectory.Length);
        }

        public IEnumerable<FileSystemEntry> GetFileSystemEntries(SearchOption searchOption)
        {
            var dirEntries = GetDirectoryEntries(searchOption);
            var fileEntries = GetFileEntries(searchOption);

            return dirEntries.Concat(fileEntries);
        }

        private IList<FileSystemEntry> GetDirectoryEntries(SearchOption searchOption)
        {
            var directories = this.FileSystem.Directory.GetDirectories(this.BaseDirectory, "*", searchOption);
            return (from d in directories
                    let dirInfo = this.FileSystem.GetDirectoryInfo(d)
                    select new NativeDirectoryEntry(dirInfo)
                    {
                        BaseDirectory = this.BaseDirectory,
                        Created = dirInfo.CreationTimeUtc.DateTimeInstance,
                        Modified = dirInfo.LastWriteTimeUtc.DateTimeInstance,
                        Name = dirInfo.Name,
                        RelativePath = GetRelativePath(dirInfo.FullName)
                    }).Cast<FileSystemEntry>().ToList();
        }

        private IList<FileSystemEntry> GetFileEntries(SearchOption searchOption)
        {
            return  (from f in this.FileSystem.Directory.GetFiles(this.BaseDirectory, "*", searchOption)
                     let fInfo = this.FileSystem.GetFileInfo(f)
                     select new NativeFileEntry(fInfo)
                     {
                         BaseDirectory = this.BaseDirectory,
                         Created = fInfo.CreationTimeUtc.DateTimeInstance,
                         Modified = fInfo.LastWriteTimeUtc.DateTimeInstance,
                         Name = fInfo.Name,
                         RelativePath = GetRelativePath(fInfo.FullName),
                         Size = fInfo.Length
                     }).Cast<FileSystemEntry>().ToList();
        }

        public FileSystemEntry Create(FileSystemEntry entry)
        {
            if (entry is FileEntry)
                return CreateFile(entry as FileEntry);
            else
                return CreateDirectory(entry as DirectoryEntry);
        }

        private NativeFileEntry CreateFile(FileEntry entry)
        {
            var fullPath = Path.Combine(this.BaseDirectory, entry.RelativePath);

            if (this.FileSystem.File.Exists(fullPath))
                this.FileSystem.File.Delete(fullPath);
            
            using (var destStream = entry.Open())
            using (var sourceStream = this.FileSystem.File.Create(fullPath).StreamInstance)
            {
                byte[] buffer = new byte[1024];
                int offset = 0;
                while (true)
                {
                    int bytesRead = destStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead <= 0)
                        break;

                    sourceStream.Write(buffer, 0, bytesRead);
                    offset += bytesRead;                    

                    if (bytesRead < buffer.Length)
                        break;
                }

                sourceStream.Flush();
            }

            var info = this.FileSystem.GetFileInfo(fullPath);

            return new NativeFileEntry(info)
            {
                BaseDirectory = this.BaseDirectory,
                Created = info.CreationTimeUtc.DateTimeInstance,
                Modified = info.LastWriteTimeUtc.DateTimeInstance,
                RelativePath = entry.RelativePath
            };
        }        

        private NativeDirectoryEntry CreateDirectory(DirectoryEntry entry)
        {
            var fullPath = Path.Combine(this.BaseDirectory, entry.RelativePath);
            if (this.FileSystem.Directory.Exists(fullPath) == false)
                this.FileSystem.Directory.CreateDirectory(fullPath);

            var info = this.FileSystem.GetDirectoryInfo(fullPath);

            return new NativeDirectoryEntry(info)
            {
                BaseDirectory = this.BaseDirectory,
                Created = info.CreationTimeUtc.DateTimeInstance,
                Modified = info.LastWriteTimeUtc.DateTimeInstance,
                RelativePath = entry.RelativePath
            };
        }

        public void Delete(FileSystemEntry entry)
        {
            var fullPath = Path.Combine(this.BaseDirectory, entry.RelativePath);

            var file = entry as FileEntry;                        

            if (file != null)
                DeleteFile(file);

            var dir = entry as DirectoryEntry;

            if (dir != null)
                DeleteDirectory(dir);
        }

        private void DeleteFile(FileEntry entry)
        {
            var fullPath = Path.Combine(this.BaseDirectory, entry.RelativePath);

            if (this.FileSystem.File.Exists(fullPath))
                this.FileSystem.File.Delete(fullPath);
        }

        private void DeleteDirectory(DirectoryEntry entry)
        {
            var fullPath = Path.Combine(this.BaseDirectory, entry.RelativePath);

            if (this.FileSystem.Directory.Exists(fullPath))
                this.FileSystem.Directory.Delete(fullPath);
        }

        public string Id { get; set; }
    }
}
