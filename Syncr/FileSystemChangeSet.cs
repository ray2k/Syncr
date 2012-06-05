using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public class FileSystemChangeSet
    {
        private List<FileSystemChange> _changesToSource = new List<FileSystemChange>();
        private List<FileSystemChange> _changesToDestination = new List<FileSystemChange>();

        public FileSystemChangeSet()
        {            
        }

        internal void AddChangeToSource(FileSystemChange change)
        {
            _changesToSource.Add(change);
        }

        internal void AddChangeToDestination(FileSystemChange change)
        {
            _changesToDestination.Add(change);
        }

        public IList<FileSystemChange> ChangesToSource 
        {
            get { return _changesToSource.AsReadOnly(); }
        }

        public IList<FileSystemChange> ChangesToDestination
        {
            get { return _changesToDestination.AsReadOnly(); }
        }
    }
}
