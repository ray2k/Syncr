using System;

namespace Syncr.FileSystems.Native.IO
{
    public interface INetworkConnectionFactory
    {
        INetworkConnection CreateConnection(string remotePath, string user, string password, string domain);
    }
}
