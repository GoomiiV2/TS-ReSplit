using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TS2
{

    // Used in models and levels
    public struct MatInfo
    {
        public const uint SIZE = 16;

        public uint ID;
        // These seem to be differn't for model than map meshes
        public WrapMode WrapModeX;
        public WrapMode WrapModeY;
        public Flag Flags;

        public static List<MatInfo> ReadMatInfos(BinaryReader R, uint Offset)
        {
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var materials = new List<MatInfo>();

            while (true)
            {
                var matInfo = Read(R);

                if (matInfo.ID != 0xFFFFFFFF)
                {
                    materials.Add(matInfo);
                }
                else { break; }
            }

            return materials;
        }

        public static MatInfo[] ReadMatInfos(BinaryReader R, uint Offset, int NumMats)
        {
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var materials = new MatInfo[NumMats];

            for (int i = 0; i < NumMats; i++)
            {
                var matInfo  = Read(R);
                materials[i] = matInfo;
            }

            return materials;
        }

        public static MatInfo Read(BinaryReader R)
        {
            var matInfo = new MatInfo()
            {
                ID        = R.ReadUInt32(),
                WrapModeX = (WrapMode)R.ReadUInt32(),
                WrapModeY = (WrapMode)R.ReadUInt32(),
                Flags     = (Flag)R.ReadUInt32()
            };

            return matInfo;
        }

        public enum WrapMode : uint
        {
            Repeat,
            NoRepeat
        }

        [Flags]
        public enum Flag : uint
        {
            None               = 0,
            TexturesIncInModel = 268435456
        }
    }
}
