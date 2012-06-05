using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Syncr;

namespace Syncr.FileSystems.Native
{
    public class NativeFileSystem : IFileSystem
    {
        protected string BaseDirectory { get; private set; }
        protected WindowsFileSystemOptions Options { get; private set; }

        public NativeFileSystem(WindowsFileSystemOptions options)
        {
            this.Options = options;
            this.BaseDirectory = options.Path.WithTrailingPathSeparator();
        }

        private string GetRelativePath(string fullPath)
        {
            return fullPath.Replace(this.BaseDirectory, string.Empty);
        }

        public IEnumerable<FileSystemEntry> GetFileSystemEntries(SearchOption searchOption)
        {
            var dirEntries = GetDirectoryEntries(searchOption);
            var fileEntries = GetFileEntries(searchOption);

            return dirEntries.Concat(fileEntries);
        }

        private IList<FileSystemEntry> GetDirectoryEntries(SearchOption searchOption)
        {
            return (from d in Directory.GetDirectories(this.BaseDirectory, "*", searchOption)
                              let dirInfo = new DirectoryInfo(d)
                              select new NativeDirectoryEntry()
                              {
                                  BaseDirectory = this.BaseDirectory,
                                  Created = dirInfo.CreationTimeUtc,
                                  Modified = dirInfo.LastWriteTimeUtc,
                                  Name = dirInfo.Name,
                                  RelativePath = GetRelativePath(dirInfo.FullName)
                              }).Cast<FileSystemEntry>().ToList();
        }

        private IList<FileSystemEntry> GetFileEntries(SearchOption searchOption)
        {
            return  (from f in Directory.GetFiles(this.BaseDirectory, "*", searchOption)
                     let fInfo = new FileInfo(f)
                     select new NativeFileEntry()
                     {
                         BaseDirectory = this.BaseDirectory,
                         Created = fInfo.CreationTimeUtc,
                         Modified = fInfo.LastWriteTimeUtc,
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

            if (File.Exists(fullPath))
                File.Delete(fullPath);
            
            using (var destStream = entry.Open())
            using (var sourceStream = File.Create(fullPath))
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

            var info = new FileInfo(fullPath);

            return new NativeFileEntry()
            {
                BaseDirectory = this.BaseDirectory,
                Created = info.CreationTimeUtc,
                Modified = info.LastWriteTimeUtc,
                RelativePath = entry.RelativePath
            };
        }        

        private NativeDirectoryEntry CreateDirectory(DirectoryEntry entry)
        {
            var fullPath = Path.Combine(this.BaseDirectory, entry.RelativePath);
            if (Directory.Exists(fullPath) == false)
                Directory.CreateDirectory(fullPath);

            var info = new DirectoryInfo(fullPath);

            return new NativeDirectoryEntry()
            {
                BaseDirectory = this.BaseDirectory,
                Created = info.CreationTimeUtc,
                Modified = info.LastWriteTimeUtc,
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

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        private void DeleteDirectory(DirectoryEntry entry)
        {
            var fullPath = Path.Combine(this.BaseDirectory, entry.RelativePath);

            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath);
        }

        public string Id { get; set; }
    }
}
