using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public class FileSystemChangeDetector : IFileSystemChangeDetector
    {
        public IEnumerable<FileSystemChange> DetermineChangesToDestination(IEnumerable<FileSystemEntry> sourceEntries, IEnumerable<FileSystemEntry> destinationEntries, ConflictBehavior conflictBehavior)
        {
            List<FileSystemChange> result = new List<FileSystemChange>();

            var destDictionary = destinationEntries.ToDictionary(p => p.RelativePath.ToLower());            

            foreach (var sourceEntry in sourceEntries)
            {
                FileSystemEntry matchingDestination = null;
                if (destDictionary.TryGetValue(sourceEntry.RelativePath.ToLower(), out matchingDestination))
                {
                    // exists on destination

                    // determine if we have a conflict                    
                    if (sourceEntry.IsInConflictWith(matchingDestination))
                    {
                        if (conflictBehavior == ConflictBehavior.Skip)
                            continue;

                        if (conflictBehavior == ConflictBehavior.TakeSource)
                            result.Add(FileSystemChange.ForOverwrite(sourceEntry));
                        
                        if (conflictBehavior == ConflictBehavior.TakeNewest &&
                            sourceEntry.Created != null && matchingDestination.Created != null && 
                            sourceEntry.Created.Value > matchingDestination.Created.Value)
                        {
                            result.Add(FileSystemChange.ForOverwrite(sourceEntry));
                            continue;
                        }

                        if (conflictBehavior == ConflictBehavior.TakeNewest &&
                            sourceEntry.Modified != null && matchingDestination.Modified != null && 
                            sourceEntry.Modified.Value > matchingDestination.Modified.Value)
                        {
                            result.Add(FileSystemChange.ForOverwrite(sourceEntry));
                            continue;
                        }
                    }
                }
                else
                {
                    // does not exist on destination
                    result.Add(FileSystemChange.ForCreate(sourceEntry));
                }
            }

            return result;
        }

        public IEnumerable<FileSystemChange> DetermineChangesToSource(IEnumerable<FileSystemEntry> sourceEntries, IEnumerable<FileSystemEntry> destinationEntries, ConflictBehavior conflictBehavior)
        {
            List<FileSystemChange> result = new List<FileSystemChange>();

            var sourceDictionary = sourceEntries.ToDictionary(p => p.RelativePath.ToLower());

            foreach (var destEntry in destinationEntries)
            {
                FileSystemEntry matchingSource = null;
                if (sourceDictionary.TryGetValue(destEntry.RelativePath.ToLower(), out matchingSource))
                {
                    // exists on source

                    // determine if we have a conflict                    
                    if (destEntry.IsInConflictWith(matchingSource))
                    {
                        if (conflictBehavior == ConflictBehavior.Skip)
                            continue;

                        if (conflictBehavior == ConflictBehavior.TakeDestination)
                            result.Add(FileSystemChange.ForOverwrite(destEntry));

                        if (conflictBehavior == ConflictBehavior.TakeNewest &&
                            destEntry.Created != null && matchingSource.Created != null &&
                            destEntry.Created.Value > matchingSource.Created.Value)
                        {
                            result.Add(FileSystemChange.ForOverwrite(destEntry));
                            continue;
                        }

                        if (conflictBehavior == ConflictBehavior.TakeNewest &&
                            destEntry.Modified != null && matchingSource.Modified != null &&
                            destEntry.Modified.Value > matchingSource.Modified.Value)
                        {
                            result.Add(FileSystemChange.ForOverwrite(destEntry));
                            continue;
                        }
                    }
                }
                else
                {
                    // does not exist on source
                    result.Add(FileSystemChange.ForCreate(destEntry));
                }
            }

            return result;
        }
    }
}
