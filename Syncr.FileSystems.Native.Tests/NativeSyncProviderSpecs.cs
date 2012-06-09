using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Syncr.FileSystems.Native.Tests
{
    public abstract class NativeSyncProviderSpec
    {
        public Mock<IFileSystem> MockFileSystem;
        
    }
}
