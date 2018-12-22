using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Static helpers to convert from raw TImesplitter mesh data to Unitys Meshes and such
// Keeping loaders decoupled for tools
public static class TSMeshUtils
{

    /*public static Mesh SubMeshToMesh(TS2.SubMesh TS2SubMesh)
    {
        var meshes = new List<CombineInstance>();
        if (TS2SubMesh.MainMesh != null)            { meshes.Add(new CombineInstance() { mesh = TS2MeshToMesh(TS2SubMesh.MainMesh), subMeshIndex = meshes.Count });          }
        if (TS2SubMesh.SecondaryMesh != null)       { meshes.Add(new CombineInstance() { mesh = TS2MeshToMesh(TS2SubMesh.SecondaryMesh), subMeshIndex = meshes.Count });     }
        if (TS2SubMesh.TransparentMesh != null)     { meshes.Add(new CombineInstance() { mesh = TS2MeshToMesh(TS2SubMesh.TransparentMesh), subMeshIndex = meshes.Count });   }

        var mesh = new Mesh();
        mesh.CombineMeshes(meshes.ToArray(), false, false, false);

        return mesh;
    }*/

    public static MeshData SubMeshToMesh(TS2.SubMesh TS2SubMesh)
    {
        return SubMeshToMesh(TS2SubMesh, MeshCreationData.Defaults);
    }

    public static MeshData SubMeshToMesh(TS2.SubMesh TS2SubMesh, MeshCreationData Options)
    {
        var mesh              = new Mesh();
        var verts             = new List<Vector3>();
        var uvs               = new List<Vector2>();
        var normals           = new List<Vector3>();
        var texData           = new List<(MeshTexMeta, List<int> Indices)>();
        var lastIndiceId      = 0;


        for (int i = 0; i < TS2SubMesh.Meshes.Length; i++)
        {
            var subMeshMesh    = TS2SubMesh.Meshes[i];

            if (subMeshMesh != null
                    && ((   TS2.SubMesh.MeshIds)i == TS2.SubMesh.MeshIds.MainMesh           && Options.CreateMainMesh
                        || (TS2.SubMesh.MeshIds)i == TS2.SubMesh.MeshIds.SecondaryMesh      && Options.CreateOverlaysMesh
                        || (TS2.SubMesh.MeshIds)i == TS2.SubMesh.MeshIds.TransparentMesh    && Options.CreateTransparentMesh)
                )
            {
                var indices        = new Dictionary<int, List<int>>();
                var rawData        = TS2MeshToRawSubMeshe(subMeshMesh, verts.Count);

                for (int eye = 0; eye < rawData.SubMeshs.Length; eye++)
                {
                    var data = rawData.SubMeshs[eye];
                    if (indices.ContainsKey(data.MatID)) {
                        indices[data.MatID].AddRange(data.Indices);
                    }
                    else {
                        indices.Add(data.MatID, data.Indices);
                    }
                }

                verts.AddRange(rawData.Verts);
                uvs.AddRange(rawData.Uvs);
                normals.AddRange(rawData.Normals);

                // Texture data
                var indiceData = indices.Select((x, idx) => (new MeshTexMeta()
                {
                    Idx           = lastIndiceId + idx,
                    TexId         = x.Key,
                    IsTransparent = (i != (int)TS2.SubMesh.MeshIds.MainMesh)
                }, x.Value)).ToArray();

                texData.AddRange(indiceData);

                lastIndiceId += indiceData.Count();
            }
        }

        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);

        var orderedIndices = texData.Select(x => x.Indices).ToArray();
        mesh.subMeshCount = orderedIndices.Length;

        for (int i = 0; i < orderedIndices.Length; i++)
        {
            var meshMat = orderedIndices[i];
            mesh.SetTriangles(meshMat, i);
        }

        mesh.RecalculateNormals();

        if (mesh.vertices != null && mesh.vertices.Count() > 0)
        {
            Unwrapping.GenerateSecondaryUVSet(mesh);
        }

        MeshUtility.Optimize(mesh);
        mesh.UploadMeshData(true);

        var meshData = new MeshData()
        {
            Mesh    = mesh,
            TexData = texData.Select(x => x.Item1).ToArray()
        };

        return meshData;
    }

    public static Mesh TS2MeshToMesh(TS2.Mesh TS2Mesh)
    {
        var mesh           = new Mesh();
        var verts          = new List<Vector3>();
        var uvs            = new List<Vector2>();
        var normals        = new List<Vector3>();
        var subMeshIndices = new Dictionary<int, List<int>>();

        var ts2Mesh = TS2Mesh;
        var rawData = TS2MeshToRawSubMeshe(ts2Mesh, verts.Count);

        for (int eye = 0; eye < rawData.SubMeshs.Length; eye++)
        {
            var data = rawData.SubMeshs[eye];

            if (subMeshIndices.ContainsKey(data.MatID))
            {
                subMeshIndices[data.MatID].AddRange(data.Indices);
            }
            else
            {
                subMeshIndices.Add(data.MatID, data.Indices);
            }
        }

        verts.AddRange(rawData.Verts);
        uvs.AddRange(rawData.Uvs);
        normals.AddRange(rawData.Normals);

        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        //mesh.SetNormals(normals);

        if (TS2Mesh.VertexColors != null)
        {
            var vertColors = new List<Color32>(verts.Count);
            for (int i = 0; i < verts.Count; i++)
            {
                byte[] rgba      = BitConverter.GetBytes(TS2Mesh.VertexColors[i]);
                var color        = new Color32(rgba[0], rgba[1], rgba[2], rgba[3]);
                vertColors.Add(color);
            }

            mesh.SetColors(vertColors);
        }

        var orderedIndices = subMeshIndices.ToList().OrderBy(x => x.Key).ToArray();
        mesh.subMeshCount  = orderedIndices.Length;

        /*foreach (var meshMat in orderedIndices)
        {
            mesh.SetTriangles(meshMat.Value, meshMat.Key);
        }*/

        for (int i = 0; i < orderedIndices.Length; i++)
        {
            var meshMat = orderedIndices[i];
            mesh.SetTriangles(meshMat.Value, i);
        }

        //mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mesh.UploadMeshData(false);

        return mesh;
    }

    // TODO: merge uvs into an atlas and avoid the sub meshes
    // and reduce code duplication
    public static Mesh TS2ModelToMesh(TS2.Model Model)
    {
        var mesh           = new Mesh();
        var verts          = new List<Vector3>();
        var uvs            = new List<Vector2>();
        var normals        = new List<Vector3>();
        var subMeshIndices = new Dictionary<int, List<int>>();

        mesh.subMeshCount = Model.Materials.Length;

        for (int i = 0; i < Model.Meshes.Length; i++)
        {
            var ts2Mesh = Model.Meshes[i];
            var rawData = TS2MeshToRawSubMeshe(ts2Mesh, verts.Count);

            for (int eye = 0; eye < rawData.SubMeshs.Length; eye++)
            {
                var data = rawData.SubMeshs[eye];

                if (subMeshIndices.ContainsKey(data.MatID)) {
                    subMeshIndices[data.MatID].AddRange(data.Indices);
                }
                else {
                    subMeshIndices.Add(data.MatID, data.Indices);
                }
            }

            verts.AddRange(rawData.Verts);
            uvs.AddRange(rawData.Uvs);
            normals.AddRange(rawData.Normals);
        }

        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);

        var orderedIndices = subMeshIndices.ToList().OrderBy(x => x.Key).ToArray();
        foreach (var meshMat in orderedIndices)
        {
            mesh.SetTriangles(meshMat.Value, meshMat.Key);
        }

        mesh.UploadMeshData(false);

        return mesh;
    }

    // Take a TS2Mesh and return a simple intermediate objebt for later conversions
    public static RawMeshData TS2MeshToRawSubMeshe(TS2.Mesh TS2Mesh, int Offset = 0)
    {
        var vertsList   = new List<Vector3>();
        var uvsList     = new List<Vector2>();
        var normalsList = new List<Vector3>();
        var subMeshs    = new List<SubMeshData>();

        if (TS2Mesh.Verts.Length > 0)
        {
            int currentIdx = 0;
            for (int i = 0; i < TS2Mesh.SubMeshDatas.Length; i++)
            {
                var subMeshData    = TS2Mesh.SubMeshDatas[i];
                var subMeshIndices = new List<int>();

                bool faceDir = false;
                int end      = subMeshData.VertsStart + subMeshData.VertsCount;
                for (int eye = subMeshData.VertsStart; eye < end; eye++)
                {
                    var vert   = TS2Mesh.Verts[eye];
                    var uv     = TS2Mesh.Uvs[eye];

                    vertsList.Add(Ts2VertToV3(vert));
                    uvsList.Add(Ts2UVWToV2(uv));

                    if (TS2Mesh.Normals.Length > 0)
                    {
                        var normal = TS2Mesh.Normals[eye];
                        normalsList.Add(Ts2VertToV3(normal));
                    }

                    var indiceOffset = currentIdx + Offset;
                    currentIdx++;

                    if (vert.Flag == 0)
                    {
                        if ((faceDir && vert.SameStrip == 1) || (!faceDir && vert.SameStrip == 0))
                        {
                            subMeshIndices.AddRange(new int[] {
                                indiceOffset - 2,
                                indiceOffset - 1,
                                indiceOffset });
                        }
                        else
                        {
                            subMeshIndices.AddRange(new int[] {
                                indiceOffset - 1,
                                indiceOffset - 2,
                                indiceOffset });
                        }
                    }
                    else
                    {
                        faceDir = true;
                    }

                    faceDir = !faceDir;
                }
                var subMesh = new SubMeshData() {
                    ID      = subMeshData.ID,
                    MatID   = subMeshData.MatID,
                    Indices = subMeshIndices
                };

                subMeshs.Add(subMesh);
            }
        }
        else
        {
            Debug.Log("No verts");
        }

        var data     = new RawMeshData() {
            Verts    = vertsList.ToArray(),
            Uvs      = uvsList.ToArray(),
            Normals  = normalsList.ToArray(),
            SubMeshs = subMeshs.ToArray()
        };

        return data;
    }

    public static Vector3 Ts2VertToV3(TS2.Vertex Vert)
    {
        var v3 = new Vector3()
        {
            x = Vert.X,
            y = Vert.Y,
            z = Vert.Z
        };

        return v3;
    }

    public static Vector2 Ts2UVWToV2(TS2.UVW UV)
    {
        var v3 = new Vector2()
        {
            x = UV.U,
            y = UV.V,
        };

        return v3;
    }

}

public class RawMeshData
{
    public Vector3[]            Verts;
    public Vector2[]            Uvs;
    public Vector3[]            Normals;
    public SubMeshData[]        SubMeshs;
}

public struct SubMeshData
{
    public ushort MatID;
    public ushort ID;
    public List<int> Indices;
}

public struct MeshData
{
    public Mesh Mesh;
    public MeshTexMeta[] TexData;
}

public struct MeshTexMeta
{
    public int Idx;
    public int TexId;
    public bool IsTransparent;
}

public struct MeshCreationData
{
    public bool CreateMainMesh;
    public bool CreateOverlaysMesh;
    public bool CreateTransparentMesh;

    public static readonly MeshCreationData Defaults = new MeshCreationData()
    {
        CreateMainMesh        = true,
        CreateOverlaysMesh    = true,
        CreateTransparentMesh = true
    };
}
