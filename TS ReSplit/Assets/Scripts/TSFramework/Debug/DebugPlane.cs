using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DebugPlane : MonoBehaviour
{
    void Start()
    {

    }

    public static GameObject CreateFromPoints(List<Vector3> Points, Color? PlaneColor = null, string Label = null)
    {
        if (!PlaneColor.HasValue) { PlaneColor = Color.magenta; }

        var gameObject               = new GameObject($"DebugPlane: {Label}");
        var meshRender               = gameObject.AddComponent<MeshRenderer>();
        var meshFilter               = gameObject.AddComponent<MeshFilter>();
        meshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRender.receiveShadows    = false;

        // Mesh
        var mesh = new Mesh();
        mesh.SetVertices(Points.ToList());
        var uvs = new Vector3[] { new Vector3(0, 0), new Vector3(1, 0), new Vector3(1, 1), new Vector3(1, 0) }.ToList();
        uvs.AddRange(new Vector3[] { new Vector3(0, 0), new Vector3(1, 0), new Vector3(1, 1), new Vector3(1, 0) });
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(new int[] { 0, 1, 2, 3,  3, 2, 1, 0 }, MeshTopology.Quads, 0); // Two for double sided, didn't feel like making a shader
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        var normal = mesh.normals[0];
        mesh.UploadMeshData(true);

        // Material
        meshFilter.mesh         = mesh;
        meshRender.materials    = new Material[1];
        /*var mat                 = (Material)AssetDatabase.LoadAssetAtPath("Assets/Mats/DebugPlane.mat", typeof(Material)); ;
        meshRender.material     = mat;*/

        // Label
        var labelObj                = new GameObject("Text");
        var textLabel               = labelObj.AddComponent<TextMesh>();
        var meshRenderText          = labelObj.GetComponent<MeshRenderer>();
        textLabel.text              = Label;
        textLabel.alignment         = TextAlignment.Center;
        textLabel.anchor            = TextAnchor.MiddleCenter;
        textLabel.characterSize     = 0.2f;
        textLabel.fontSize          = 32;
        labelObj.transform.position = mesh.bounds.center;
        labelObj.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);

        //meshRenderText.materials     = new Material[1];
        /*var textMat                    = (Material)AssetDatabase.LoadAssetAtPath("Assets/Mats/DebugPlaneText.mat", typeof(Material));
        meshRenderText.material.shader = textMat.shader;*/

        labelObj.transform.SetParent(gameObject.transform);

        return gameObject;
    }
}
