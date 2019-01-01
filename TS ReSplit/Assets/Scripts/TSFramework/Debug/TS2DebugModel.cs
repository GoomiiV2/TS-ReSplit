using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TSDebug
{
    public enum ModelDrawMode
    {
        Flat,
        ColorByMesh,
        ColorByTriStrip
    }

    public struct ModelTextureData
    {
        public string Name;
        public int Width;
        public int Height;
        public Texture2D Texture;
    }

    public class TS2DebugModel : MonoBehaviour
    {

        // ToDO: Make an assetmanager
        public string PakPath;
        public string ModelPath;
        public int MeshToDraw = -1; // -1 Will draw all
        public ModelDrawMode DrawMode;
        public bool ShowTexturesOnScreen;
        public bool DrawBounds;

        private FPPak PakFile;
        private TS2.Model Ts2Model;
        private List<ModelTextureData> ModelTextures;

        // Use this for initialization
        void Start()
        {
            PakFile = new FPPak(PakPath);
            ModelTextures = new List<ModelTextureData>();

            CreateLineMaterial();
            LoadModel();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnRenderObject()
        {
            DrawModel();
        }

        private void OnDrawGizmos()
        {
            if (Ts2Model != null)
            {
                DrawModel();
            }
        }

        private void OnGUI()
        {
            DrawTextures();
        }

        public void LoadModel()
        {
            var testModel = PakFile.Files.Where(x => x.Name.Contains(ModelPath)).ToList()[0];
            Ts2Model = new TS2.Model(testModel.Data);

            var texIds = Ts2Model.Materials.Select(x => x.ID).ToList();
            var texFiles = PakFile.Files.Where(y => texIds.Any(a => y.Name.Contains($"{a}"))).ToList();

            foreach (var texFile in texFiles)
            {
                var texture = new TS2.Texture(texFile.Data);
                var texData = new ModelTextureData()
                {
                    Name = texFile.Name,
                    Width = texture.Width,
                    Height = texture.Height,
                    Texture = TSTextureUtils.TS2TexToT2D(texture)
                };

                ModelTextures.Add(texData);
            }
        }

        static Material lineMaterial;
        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        public void DrawModel()
        {
            lineMaterial.SetPass(0);

            if (MeshToDraw == -1)
            {
                for (int i = 0; i < Ts2Model.Meshes.Length; i++)
                {
                    var mesh = Ts2Model.Meshes[i];
                    //DrawMesh(mesh);
                }
            }
            else
            {
                var mesh = Ts2Model.Meshes[MeshToDraw];
                //DrawMesh(mesh);
            }
        }

        public void DrawMesh(TS2.Mesh Mesh)
        {
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.TRIANGLE_STRIP);

            if (DrawMode == ModelDrawMode.Flat)
            {
                GL.Color(Color.white);
            }
            else if (DrawMode == ModelDrawMode.ColorByMesh)
            {
                GL.Color(Random.ColorHSV());
            }

            byte lastFlag = Mesh.Verts[0].Flag;
            for (int i = 0; i < Mesh.Verts.Length; i++)
            {
                var vert = Mesh.Verts[i];

                if (lastFlag != 128 && vert.Flag == 128)
                {
                    GL.End();
                    GL.Begin(GL.TRIANGLE_STRIP);
                    //GL.Color(Random.ColorHSV());
                }

                lastFlag = vert.Flag;

                GL.Vertex3(vert.X, vert.Y, vert.Z);
            }

            GL.End();
            GL.PopMatrix();
        }

        public void DrawBoundingBox()
        {
            if (!DrawBounds) { return; }

        }

        public void DrawTextures()
        {
            if (!ShowTexturesOnScreen) { return; }

            const int spacing = 2;
            const int maxWidth = 300;
            int x = 0;
            int y = 0;

            for (int i = 0; i < ModelTextures.Count; i++)
            {
                var texData = ModelTextures[i];

                if ((x + texData.Width) > maxWidth)
                {
                    y += texData.Height + spacing;
                    x = 0;
                }

                DrawTexture(x, y, texData, i);
                x += texData.Width + spacing;
            }
        }

        public void DrawTexture(int x, int y, ModelTextureData TexData, int Idx)
        {
            float aspect = TexData.Width / TexData.Height;
            var rect = new Rect(x, y, TexData.Width, TexData.Height);

            GUI.DrawTexture(rect, TexData.Texture, ScaleMode.ScaleToFit, false, aspect);

            GUI.Label(rect, $"ID: {Idx} ({TexData.Width} / {TexData.Height})");
        }
    }
}
