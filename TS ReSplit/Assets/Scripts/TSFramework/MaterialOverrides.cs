using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TSFramework
{
    // Handles maping some textures to special materials
    public class MaterialOverrides
    {

    }

    [CreateAssetMenu(fileName = "MatOverride", menuName = "Resplit/Material Override")]
    public class MatOverride : ScriptableObject
    {
        public uint TextureID;
        public Material Material;
    }
}
