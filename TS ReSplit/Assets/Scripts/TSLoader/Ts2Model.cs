using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TS2
{
    public class Model
    {
        public MatInfo[] Materials;
        public Mesh[] Meshes;

        public Model() { }

        public Model(byte[] Data)
        {
            Load(Data);
        }

        public void Load(byte[] Data)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(Data)))
            {
                uint materialInfoOffset = r.ReadUInt32();
                uint infoOffset         = r.ReadUInt32();

                LoadMatInfos(r, materialInfoOffset);
                LoadMeshes(r, infoOffset);
            }
        }

        private void LoadMatInfos(BinaryReader R, uint Offset)
        {
            var materials = MatInfo.ReadMatInfos(R, Offset);
            Materials     = materials.ToArray();
        }

        private void LoadMeshes(BinaryReader R, uint Offset)
        {
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);

            uint meshCount         = R.ReadUInt32();
            uint meshInfoChunkSize = meshCount * MeshInfo.SIZE;
            uint meshInfoOffset    = Offset - meshInfoChunkSize;

            R.BaseStream.Seek(meshInfoOffset, SeekOrigin.Begin);

            var meshInfos = new List<MeshInfo>();
            for (int i = 0; i < meshCount; i++)
            {
                var meshInfo = MeshInfo.Read(R);
                meshInfos.Add(meshInfo);
            }

            var meshes = new List<Mesh>();
            foreach (var meshInfo in meshInfos)
            {
                var mesh = Mesh.Load(R, meshInfo);
                meshes.Add(mesh);
            }

            Meshes = meshes.ToArray();
        }
    }

    public class ModelHeader
    {
        public uint MaterialInfoOffset;
        public uint InfoOffset;
    }

}
