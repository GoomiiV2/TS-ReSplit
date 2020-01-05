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

    public static MeshData SubMeshToMesh(TS2.SubMesh TS2SubMesh)
    {
        return SubMeshToMesh(TS2SubMesh, MeshCreationData.Defaults);
    }

    public static MeshData SubMeshToMesh(TS2.SubMesh TS2SubMesh, MeshCreationData Options)
    {
        var meshArr = new TS2.SubMesh[] { TS2SubMesh };
        return SubMeshToMesh(meshArr, Options);
    }

    // TODO: prob a good canadate for a tidy up and refactor
    public static MeshData SubMeshToMesh(TS2.SubMesh[] TS2SubMesh, MeshCreationData Options, TSData.TS2ModelInfo ModelInfo = null)
    {
        var meshToBone = (ModelInfo != null && ModelInfo.BoneToMehses.HasValue) ? ModelInfo.BoneToMehses.Value.ToLookup() : new Dictionary<int, int>();

        var mesh              = new Mesh();
        var verts             = new List<Vector3>();
        var uvs               = new List<Vector2>();
        var normals           = new List<Vector3>();
        var texData           = new List<(MeshTexMeta, List<int> Indices)>();
        var weights           = new List<BoneWeight>();
        var lastIndiceId      = 0;

        var vertLookup = new Dictionary<Vector3, int>();


        for (int aye = 0; aye < TS2SubMesh.Length; aye++)
        {
            var currMesh           = TS2SubMesh[aye];
            var shouldntSkipThisMesh = (ModelInfo == null || ModelInfo.IngoreMeshes == null || !ModelInfo.IngoreMeshes.Contains(aye));
            
                for (int i = 0; i < currMesh.Meshes.Length; i++)
                {
                    var subMeshMesh = currMesh.Meshes[i];

                    if (subMeshMesh != null &&
                            ((   TS2.SubMesh.MeshIds)i == TS2.SubMesh.MeshIds.MainMesh           && Options.CreateMainMesh
                            || (TS2.SubMesh.MeshIds)i == TS2.SubMesh.MeshIds.SecondaryMesh      && Options.CreateOverlaysMesh
                            || (TS2.SubMesh.MeshIds)i == TS2.SubMesh.MeshIds.TransparentMesh    && Options.CreateTransparentMesh)
                        )
                    {
                        var indices = new Dictionary<int, List<int>>();
                        var rawData = TS2MeshToRawSubMeshe(subMeshMesh, verts.Count);

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

                        if (shouldntSkipThisMesh)
                        {
                            verts.AddRange(rawData.Verts);
                            uvs.AddRange(rawData.Uvs);
                            normals.AddRange(rawData.Normals);

                            if (Options.IsSkeletalMesh)
                            {
                                var hasBoneIdx  = meshToBone.TryGetValue(aye, out int boneIdx);
                                var meshWeights = rawData.Weights.Select((x, idx) => new BoneWeight()
                                {
                                    boneIndex0 = boneIdx,
                                    weight0    = x,
                                });
                                weights.AddRange(meshWeights);
                            }
                        }

                        // Texture data
                        var indiceData = indices.Select((x, idx) => (new MeshTexMeta()
                        {
                            Idx           = lastIndiceId + idx,
                            TexId         = x.Key,
                            IsTransparent = (i != (int)TS2.SubMesh.MeshIds.MainMesh)
                        }, x.Value)).ToArray();

                        texData.AddRange(indiceData);

                        if (shouldntSkipThisMesh) { lastIndiceId += indiceData.Count(); }
                    }
                }
            
        }

        mesh.SetVertices(verts);
        if (normals.Count > 0) { mesh.SetNormals(normals); }
        mesh.SetUVs(0, uvs);

        // This is a bit messy and prob allocate heavy
        // TODO: Worth revisting this and speeding it up, probaly
        if (Options.IsSkeletalMesh)
        {
            var weightsArr   = weights.ToArray();
            var combined     = verts.Select((x, i) =>  (Vertex: x, Weight: weights[i], Idx: i));
            var groupedVerts = combined.GroupBy(x => x.Vertex);

            foreach (var group in groupedVerts)
            {
                var first = group.First();
                var last  = group.Last();

                foreach (var vert in group)
                {
                    weightsArr[vert.Idx].boneIndex0 = first.Weight.boneIndex0;
                    weightsArr[vert.Idx].weight0 = 0.5f;
                    weightsArr[vert.Idx].boneIndex1 = last.Weight.boneIndex0;
                    weightsArr[vert.Idx].weight1 = 0.5f;
                }
            }

            mesh.boneWeights = weightsArr;
        }

        //TODO: tidy up
        if (!Options.IsMapMesh) // Models need the ids to be in order and can have
        {
            var mergedIndices = new List<(MeshTexMeta, List<int> Indices)>();
            foreach (var tData in texData.GroupBy(x => x.Item1.TexId))
            {
                var indices = tData.SelectMany(x => x.Indices);
                mergedIndices.Add((new MeshTexMeta()
                {
                    Idx           = mergedIndices.Count(),
                    TexId         = tData.First().Item1.TexId,
                    IsTransparent = tData.First().Item1.IsTransparent
                }, indices.ToList()));
            }

            texData = mergedIndices.OrderBy(x => x.Item1.TexId).ToList();
        }

        var orderedIndices = texData.Select(x => x.Indices).ToArray();
        mesh.subMeshCount  = orderedIndices.Length;

        for (int i = 0; i < orderedIndices.Length; i++)
        {
            var meshMat = orderedIndices[i];
            mesh.SetTriangles(meshMat, i);
        }

        if (Options.IsMapMesh)
        {
            mesh.RecalculateNormals();
            if (Options.IsMapMesh && mesh.vertices != null && mesh.vertices.Count() > 0)
            {
#if UNITY_EDITOR
                // TODO: Find or well more likey make a runtime alterntive for this since its locked away as editor only :<
                //var vertsBefore   = mesh.vertices;
                Unwrapping.GenerateSecondaryUVSet(mesh);
                //var diffVerts = mesh.vertices.Except(vertsBefore).ToArray();
#endif

                //CreateUV(ref mesh);
            }
        }

        mesh.UploadMeshData(false);

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

    public static Mesh TS2ModelToMesh(TS2.Model Model)
    {
        var data = SubMeshToMesh(Model.Meshes, new MeshCreationData()
        {
            CreateMainMesh        = true,
            CreateOverlaysMesh    = true,
            CreateTransparentMesh = true
        });

        return data.Mesh;
    }

    // Take a TS2Mesh and return a simple intermediate objebt for later conversions
    public static RawMeshData TS2MeshToRawSubMeshe(TS2.Mesh TS2Mesh, int Offset = 0)
    {
        var vertsList   = new List<Vector3>();
        var uvsList     = new List<Vector2>();
        var normalsList = new List<Vector3>();
        var subMeshs    = new List<SubMeshData>();
        var weights     = new List<float>();

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
                    weights.Add(uv.W);

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
            SubMeshs = subMeshs.ToArray(),
            Weights  = weights.ToArray()
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

    public static Vector2[] CreateUV(ref Mesh mesh)
    {

        int i = 0;
        Vector3 p = Vector3.up;
        Vector3 u = Vector3.Cross(p, Vector3.forward);
        if (Vector3.Dot(u, u) < 0.001f)
        {
            u = Vector3.right;
        }
        else
        {
            u = Vector3.Normalize(u);
        }

        Vector3 v = Vector3.Normalize(Vector3.Cross(p, u));
        Vector3[] vertexs = mesh.vertices;
        int[] tris = mesh.triangles;
        Vector2[] uvs = new Vector2[vertexs.Length];

        for (i = 0; i < tris.Length; i += 3)
        {

            Vector3 a = vertexs[tris[i]];
            Vector3 b = vertexs[tris[i + 1]];
            Vector3 c = vertexs[tris[i + 2]];
            Vector3 side1 = b - a;
            Vector3 side2 = c - a;
            Vector3 N = Vector3.Cross(side1, side2);

            N = new Vector3(Mathf.Abs(N.normalized.x), Mathf.Abs(N.normalized.y), Mathf.Abs(N.normalized.z));



            if (N.x > N.y && N.x > N.z)
            {
                uvs[tris[i]] = new Vector2(vertexs[tris[i]].z, vertexs[tris[i]].y);
                uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].z, vertexs[tris[i + 1]].y);
                uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].z, vertexs[tris[i + 2]].y);
            }
            else if (N.y > N.x && N.y > N.z)
            {
                uvs[tris[i]] = new Vector2(vertexs[tris[i]].x, vertexs[tris[i]].z);
                uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].z);
                uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].z);
            }
            else if (N.z > N.x && N.z > N.y)
            {
                uvs[tris[i]] = new Vector2(vertexs[tris[i]].x, vertexs[tris[i]].y);
                uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].y);
                uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].y);
            }

        }

        // Get the uvs in 0 to 1 range
        var uvsX = uvs.Select(x => x.x);
        var uvsY = uvs.Select(x => x.y);
        var min  = new Vector2(uvsX.Min(), uvsY.Min());
        var max  = new Vector2(uvsX.Max(), uvsY.Max());

        float Map(float a1, float a2, float b1, float b2, float s) => b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        for (int idx = 0; idx < uvs.Length; idx++)
        {
            uvs[idx].x = Map(min.x + min.x, max.x, 0, 1, min.x + uvs[idx].x);
            uvs[idx].y = Map(min.y + min.y, max.y, 0, 1, min.y + uvs[idx].y);
        }

        mesh.uv2 = uvs;
        return uvs;
    }
}

public class RawMeshData
{
    public Vector3[]            Verts;
    public Vector2[]            Uvs;
    public Vector3[]            Normals;
    public SubMeshData[]        SubMeshs;
    public float[]              Weights;
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
    public bool IsMapMesh;
    public bool IsSkeletalMesh;

    public static readonly MeshCreationData Defaults = new MeshCreationData()
    {
        CreateMainMesh        = true,
        CreateOverlaysMesh    = true,
        CreateTransparentMesh = true,
        IsMapMesh             = true,
        IsSkeletalMesh        = false
    };
}