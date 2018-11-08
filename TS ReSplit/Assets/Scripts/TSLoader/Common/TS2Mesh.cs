using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS2
{
    public class Mesh
    {
        public Vertex[] Verts;
        public UVW[] Uvs;
        public Vertex[] Normals;
        public SubMeshData[] SubMeshDatas;

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

        public static Mesh Load(BinaryReader R, MeshInfo Info)
        {
            var mesh = new Mesh();

            // Verts
            R.BaseStream.Seek(Info.VertsOffset, SeekOrigin.Begin);

            var verts = new List<Vertex>();
            for (int i = 0; i < Info.NumVerts; i++)
            {
                var vert = Vertex.Read(R);
                verts.Add(vert);
            }

            // UVs 
            R.BaseStream.Seek(Info.UvsOffset, SeekOrigin.Begin);

            var uvs = new List<UVW>();
            for (int i = 0; i < Info.NumVerts; i++)
            {
                var uvw = UVW.Read(R);
                uvs.Add(uvw);
            }

            // Normals
            R.BaseStream.Seek(Info.NormalsOffset, SeekOrigin.Begin);

            var normals = new List<Vertex>();
            for (int i = 0; i < Info.NumVerts; i++)
            {
                var normal = Vertex.Read(R);
                normals.Add(normal);
            }

            // Submesh details
            R.BaseStream.Seek(Info.MatIdOffset, SeekOrigin.Begin);

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

            mesh.Verts = verts.ToArray();
            mesh.Uvs = uvs.ToArray();
            mesh.Normals = normals.ToArray();
            mesh.SubMeshDatas = subMeshDetails.ToArray();

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
