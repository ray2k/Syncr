using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Syncr
{
    public class SyncronizationOptions
    {
        public SyncDirection SyncDirection { get; set; }
        public SearchOption SearchOption { get; set; }
        public TimeSpan PollingInterval { get; set; }
        public ConflictBehavior ConflictBehavior { get; set; }
    }
}
