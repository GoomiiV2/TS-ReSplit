using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitterV2
{
    public class PakFileListing
    {
        public struct PakInfo
        {
            public string PakMagic;
            public string PakName;
        }

        public PakInfo PakFileInfo;
        public string[] Files;
    }
}
