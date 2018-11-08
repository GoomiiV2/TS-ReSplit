using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS2
{
    public struct UVW
    {
        public float U;
        public float V;
        public float W;

        public static UVW Read(BinaryReader R)
        {
            var uvw = new UVW() // OWO
            {
                U = R.ReadSingle(),
                V = R.ReadSingle(),
                W = R.ReadSingle()
            };

            return uvw;
        }
    }
}
