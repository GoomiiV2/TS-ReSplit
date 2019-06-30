using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TS2
{
    public class Model
    {
        public MatInfo[] Materials;
        public SubMesh[] Meshes;
        public Texture[] Textures;
        public MeshInfo[] MeshInfos;

        public float Scale;

        public bool HasIncludedTextures { get { return Textures != null; } }

        public Model() { }

        public Model(byte[] Data)
        {
            Load(Data);
        }

        public void Load(byte[] Data)
        {
            const uint MODEL_INFO_SIZE = 48;

            using (BinaryReader r = new BinaryReader(new MemoryStream(Data)))
            {
                uint materialInfoOffset = r.ReadUInt32();
                uint infoOffset         = r.ReadUInt32();

                LoadMatInfos(r, materialInfoOffset);
                LoadMeshes(r, infoOffset);

                // if there is more data, should be textures included in the model
                uint includedTexOffset = infoOffset + MODEL_INFO_SIZE;
                if (r.BaseStream.Length > includedTexOffset)
                {
                    LoadTextures(r, includedTexOffset);
                }
            }

            foreach (var mesh in Meshes)
            {

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

            uint meshCount = R.ReadUInt32();
            R.BaseStream.Seek(36, SeekOrigin.Current);
            Scale = R.ReadSingle();

            uint meshInfoChunkSize = meshCount * MeshInfo.SIZE;
            uint meshInfoOffset    = Offset - meshInfoChunkSize;

            R.BaseStream.Seek(meshInfoOffset, SeekOrigin.Begin);

            var meshInfos = new List<MeshInfo>();
            for (int i = 0; i < meshCount; i++)
            {
                var meshInfo = MeshInfo.Read(R);
                meshInfos.Add(meshInfo);
            }

            var meshes = new List<SubMesh>();
            foreach (var meshInfo in meshInfos)
            {
                var mesh = SubMesh.Load(R, meshInfo);
                meshes.Add(mesh);
            }

            Meshes    = meshes.ToArray();
            MeshInfos = meshInfos.ToArray();
        }

        private void LoadTextures(BinaryReader R, uint Offset)
        {
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);
            Textures = new Texture[Materials.Length];

            for (int i = 0; i < Textures.Length; i++)
            {
                var tex     = Texture.Read(R);
                Textures[i] = tex;
            }
        }
    }
}
