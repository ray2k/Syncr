using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface IFileSystemChangeDetector
    {
        IEnumerable<FileSystemChange> DetermineChangesToDestination(IEnumerable<FileSystemEntry> sourceEntries, IEnumerable<FileSystemEntry> destinationEntries, ConflictBehavior conflictBehavior);
        IEnumerable<FileSystemChange> DetermineChangesToSource(IEnumerable<FileSystemEntry> sourceEntries, IEnumerable<FileSystemEntry> destinationEntries, ConflictBehavior conflictBehavior);
    }
}
