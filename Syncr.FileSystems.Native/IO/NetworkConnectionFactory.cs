using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Syncr.FileSystems.Native.IO
{
    public sealed class NetworkConnectionFactory : Syncr.FileSystems.Native.IO.INetworkConnectionFactory
    {
        public INetworkConnection CreateConnection(string remotePath, string user, string password)
        {
            NetworkCredential credentials = null;

            credentials = new NetworkCredential(user, password);

            return new NetworkConnection(remotePath, credentials);
        }
    }
}
