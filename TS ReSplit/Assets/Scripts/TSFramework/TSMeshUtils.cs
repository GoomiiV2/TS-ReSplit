using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Static helpers to convert from raw TImesplitter mesh data to Unitys Meshes and such
// Keeping loaders decoupled for tools
public static class TSMeshUtils
{

    public static Mesh TS2MeshToMesh(TS2.Mesh TS2Mesh, int NumTextures)
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

        var orderedIndices = subMeshIndices.ToList().OrderBy(x => x.Key).ToArray();
        mesh.subMeshCount  = NumTextures;

        foreach (var meshMat in orderedIndices)
        {
            mesh.SetTriangles(meshMat.Value, meshMat.Key);
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
                    var normal = TS2Mesh.Normals[eye];

                    vertsList.Add(Ts2VertToV3(vert));
                    uvsList.Add(Ts2UVWToV2(uv));
                    normalsList.Add(Ts2VertToV3(normal));

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
