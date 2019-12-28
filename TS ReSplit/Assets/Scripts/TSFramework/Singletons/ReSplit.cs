using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSFramework.Singletons;

namespace Assets.Scripts.TSFramework.Singletons
{
    // Main singleton handler
    public static class ReSplit
    {
        public static Cache Cache { get; private set; } = new Cache();
    }
}
