using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncr.FileSystems.Native.Tests
{
    public class FakeDirectoryEntry : DirectoryEntry
    {
        public override bool CanWriteCreationTime
        {
            get { return true; }
        }

        public override bool CanWriteModificationTime
        {
            get { return true; }
        }        
    }
}
