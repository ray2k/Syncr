using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncr
{
    public enum ConflictBehavior
    {
        TakeSource = 0,
        TakeDestination = 1,        
        Skip = 3,
        TakeNewest = 4
    }
}
