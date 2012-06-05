using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public interface IFileSystemProvider
    {
        IFileSystem CreateFileSystem(object options);
        object CreateDefaultOptions();
        string Name { get; }
        string Description { get; }
    }
}
