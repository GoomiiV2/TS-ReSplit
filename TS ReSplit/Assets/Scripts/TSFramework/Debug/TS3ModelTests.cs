using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests.Debug
{
    class TS3ModelTests : MonoBehaviour
    {
        public string ModelPath;

        public void Start()
        {
            var meshData = TSAssetManager.LoadFile(ModelPath);
            var ts3Mesh  = new TS3.Model(meshData);
        }
    }
}
