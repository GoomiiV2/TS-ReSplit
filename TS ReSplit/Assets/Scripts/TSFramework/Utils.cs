using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TSFramework
{
    public static class Utils
    {

        public static Vector3 V3FromFloats(float[] Floats)
        {
            var vector = new Vector3(Floats[0], Floats[1], Floats[2]);
            return vector;
        }
    }
}
