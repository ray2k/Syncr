using System;

namespace Syncr.FileSystems.Native.IO
{
    public interface INetworkConnection : IDisposable
    {
        void Connect();
    }
}
