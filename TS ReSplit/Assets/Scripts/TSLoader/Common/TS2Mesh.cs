using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS;

namespace TS2
{
    public struct SubMesh
    {
        public enum MeshIds
        {
            MainMesh,
            SecondaryMesh,
            TransparentMesh
        };

        public Mesh[] Meshes;

        public Mesh MainMesh            { get { return Meshes[(int)MeshIds.MainMesh];           } }
        public Mesh SecondaryMesh       { get { return Meshes[(int)MeshIds.SecondaryMesh];      } }
        public Mesh TransparentMesh     { get { return Meshes[(int)MeshIds.TransparentMesh];    } }

        public uint[] GetTextureIds(MatInfo[] MatInfos)
        {
            var uids = new List<uint>(MatInfos.Length);
            for (int i = 0; i < Meshes.Length; i++)
            {
                var mesh = Meshes[i];
                if (mesh != null)
                {
                    var ids = mesh.GetTextureIds(MatInfos);
                    for (int eye = 0; eye < ids.Length; eye++)
                    {
                        if (!uids.Contains(ids[eye]))
                        {
                            uids.Add(ids[eye]);
                        }
                    }
                }
            }

            return uids.ToArray();
        }

        public static SubMesh Load(BinaryReader R, MeshInfo Info)
        {
            var subMesh = new SubMesh();
            subMesh.Meshes = new Mesh[]
            {
                Mesh.Load(R, Info.MeshOffsets),
                Mesh.Load(R, Info.MeshOffsets2),
                Mesh.Load(R, Info.TransparentMeshOffsets)
            };

            return subMesh;
        }
    }

    public class Mesh
    {
        public Vertex[] Verts;
        public UVW[] Uvs;
        public Vertex[] Normals;
        public uint[] VertexColors;
        public SubMeshData[] SubMeshDatas;
        public MatInfo[] MatInfos;

        public Vertex[][] GetTristrips()
        {
            var triStrips = new List<Vertex[]>();
            var strip = new List<Vertex>();
            byte lastStripIndicator = Verts[0].SameStrip;

            foreach (var vert in Verts)
            {
                if (lastStripIndicator != vert.SameStrip)
                {
                    var triStripArray = strip.ToArray();
                    triStrips.Add(triStripArray);
                    strip.Clear();

                    lastStripIndicator = vert.SameStrip;
                }

                strip.Add(vert);
            }

            var triStripsArray = triStrips.ToArray();
            return triStripsArray;
        }

        // Returns a list of the texture ids that are actually used on this mesh
        public uint[] GetTextureIds(MatInfo[] MatInfos)
        {
            var uniqueMats = SubMeshDatas.DistinctBy(x => x.MatID).OrderBy(x => x.MatID).ToArray();
            var uniqueIds  = new uint[uniqueMats.Count()];

            for (uint i = 0; i < uniqueMats.Count(); i++)
            {
                var matId    = uniqueMats[i].MatID;

                try
                {
                    if (i < MatInfos.Length)
                    {
                        uniqueIds[i] = MatInfos[matId].ID;
                    }
                    else
                    {
                        uniqueIds[i] = uint.MaxValue; // Force the missing texture to be used
                    }
                }
                catch (Exception e)
                {

                }
            }

            return uniqueIds;
        }

        public static Mesh Load(BinaryReader R, MeshInfoOffsets Info)
        {
            if (Info.MatRanges == 0) { return null; }

            var mesh               = new Mesh();

            // Mat infos
            var matListSize = Info.Verts - Info.Offset;
            if (matListSize > 0)
            {
                var numMats      = (matListSize) / MatInfo.SIZE;
                mesh.MatInfos    = MatInfo.ReadMatInfos(R, Info.Offset, (int)numMats);
            }

            // Verts
            R.BaseStream.Seek(Info.Verts, SeekOrigin.Begin);
            var verts = new List<Vertex>((int)Info.NumVerts);
            for (int i = 0; i < Info.NumVerts; i++)
            {
                var vert = Vertex.Read(R);
                verts.Add(vert);
            }

            // UVs 
            R.BaseStream.Seek(Info.Uvs, SeekOrigin.Begin);
            var uvs = new List<UVW>((int)Info.NumVerts);
            for (int i = 0; i < Info.NumVerts; i++)
            {
                var uvw = UVW.Read(R);
                uvs.Add(uvw);
            }

            // Vertex Colors
            var vertColors = new List<uint>((int)Info.NumVerts);
            if (Info.VertexColors != 0)
            {
                R.BaseStream.Seek(Info.VertexColors, SeekOrigin.Begin);

                for (int i = 0; i < Info.NumVerts; i++)
                {
                    var color = R.ReadUInt32();
                    vertColors.Add(color);
                }
            }

            // Normals
            var normals = new List<Vertex>((int)Info.NumVerts);
            if (Info.Normals != 0)
            {
                R.BaseStream.Seek(Info.Normals, SeekOrigin.Begin);

                for (int i = 0; i < Info.NumVerts; i++)
                {
                    var normal = Vertex.Read(R);
                    normals.Add(normal);
                }
            }

            // Submesh details
            R.BaseStream.Seek(Info.MatRanges, SeekOrigin.Begin);
            var subMeshDetails = new List<SubMeshData>();
            while (true)
            {
                var subMesh   = SubMeshData.Read(R);
                var endMarker = R.ReadUInt16();

                if (endMarker != 0xFFFF)
                {
                    subMeshDetails.Add(subMesh);
                }
                else
                {
                    break;
                }
            }

            mesh.Verts        = verts.ToArray();
            mesh.Uvs          = uvs.ToArray();
            mesh.Normals      = normals.ToArray();
            mesh.SubMeshDatas = subMeshDetails.ToArray();
            mesh.VertexColors = vertColors.ToArray();

            return mesh;
        }
    }

    public struct SubMeshData
    {
        public ushort MatID;
        public ushort ID;
        public ushort VertsStart;
        public ushort VertsCount;

        public static SubMeshData Read(BinaryReader R)
        {
            var subMesh = new SubMeshData()
            {
                MatID      = R.ReadUInt16(),
                ID         = R.ReadUInt16(),
                VertsStart = R.ReadUInt16(),
                VertsCount = R.ReadUInt16()
            };

            return subMesh;
        }
    }
}
