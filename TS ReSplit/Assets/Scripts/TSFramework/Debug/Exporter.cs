using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGLTF;

namespace Assets.Scripts.TSFramework.Debug
{
    // Alot of this is fragile, but its mostly extra side features so eh
    public static class Exporter
    {

        public static void ExportAllChrModels()
        {
            var modelPak = "ts2/pak/chr.pak";
            var chrFiles = TSAssetManager.GetFileListForPak(modelPak).Select(x => (modelPak, modelPak + "/" + x)).Where(x => !x.Item2.Contains("textures"));
            foreach (var chrFile in chrFiles)
            {
                try
                {
                    ExportModel(chrFile.Item2);
                }
                catch { }
            }
        }

        public static void ExportModel(string ModelPath, bool RenameBones = false, bool WithAnimations = false)
        {
            var modelName = Path.GetFileNameWithoutExtension(ModelPath);
            var outPath = Path.Combine($"{Application.dataPath}../../", "Export", $"{(WithAnimations ? (modelName + "Animations") : modelName)}");
            Directory.CreateDirectory(outPath);

            var go        = new GameObject("Model");
            var aniModel  = go.AddComponent<AnimatedModelV2>();
            var anminComp = go.AddComponent<Animation>();
            aniModel.LoadModel(ModelPath, null);
            if (RenameBones) { RenameModelBones(go); }

            if (WithAnimations)
            {
                var animationPaks = new string[] { "ts2/pak/anim.pak" };
                foreach (var animPak in animationPaks)
                {
                    var files    = TSAssetManager.GetFileListForPak(animPak).Select(x => Path.Combine(animPak, x));
                    foreach (var file in files)
                    {
                        try
                        {
                            var name     = Path.GetFileNameWithoutExtension(file);
                            var animData = TSAssetManager.LoadFile(file);
                            var ts2Anim  = new TS2.Animation(animData);
                            var scale    = aniModel.ModelScale;
                            var clip     = TSAnimationUtils.ConvertAnimation(ts2Anim, TS2AnimationData.HumanSkel, name, UseRootMotion: true, IsLooping: false, Scale: scale);

                            anminComp.AddClip(clip, name);
                        }
                        catch { }
                    }
                }
            }

            try
            {
                var exporter = new GLTFEditorExporter(new Transform[] { go.transform });
                exporter.enableAnimation(true);
                exporter.SaveGLTFandBin(outPath, modelName);
            }
            catch { }

            var meshRender = go.GetComponent<SkinnedMeshRenderer>();
            ExportTextures(meshRender.materials, outPath);

            GameObject.Destroy(go);
        }

        public static void ExportLevel(string LevelPath)
        {
            const string EXPORT_SCENE_NAME = "ExportScene";
            // export.level ts2/pak/story/l_10_ST.pak

            int.TryParse(Path.GetFileNameWithoutExtension(LevelPath).Split('_')[1], out int levelID);
            var filePath = $"{LevelPath}/bg/level{levelID}/level{levelID}.raw";
            var fileName = Path.GetFileNameWithoutExtension(LevelPath);

            var levelManagerPrefab = Resources.Load<GameObject>("Prefabs/LevelManager");
            var spawnedGO          = GameObject.Instantiate(levelManagerPrefab);
            var manager            = spawnedGO.GetComponent<TS2Level>();

            manager.LevelID = $"{levelID}";
            manager.LevelPak = LevelPath;
            manager.Start();

            // And now export it
            try
            {
                var outPath = Path.Combine($"{Application.dataPath}../../", "Export", "Levels", fileName);
                Directory.CreateDirectory(outPath);

                var levelBase = GameObject.Find(TS2Level.BASE_GO_NAME);
                var exporter = new GLTFEditorExporter(new Transform[] { levelBase.transform });
                exporter.enableAnimation(true);
                exporter.SaveGLTFandBin(outPath, fileName);

                var meshRender = levelBase.GetComponent<SkinnedMeshRenderer>();
                ExportTextures(meshRender.materials, outPath);

                GameObject.DestroyObject(levelBase);
            }
            catch { }
        }

        private static void RenameModelBones(GameObject Go)
        {
            for (int i = 0; i < Go.transform.childCount; i++)
            {
                var child = Go.transform.GetChild(i);

                switch (child.name.ToLower())
                {
                    case "root":                child.name = "base";        break;

                    case "right shoulder 1":    child.name = "rblade";      break;
                    case "right shoulder 2":    child.name = "rshoulder";   break;
                    case "right elbow":         child.name = "relbow";      break;
                    case "right wrist":         child.name = "rwrist";      break;

                    case "left shoulder 1":     child.name = "lblade";      break;
                    case "left shoulder 2":     child.name = "lshoulder";   break;
                    case "left elbow":          child.name = "lelbow";      break;
                    case "left wrist":          child.name = "lwrist";      break;

                    case "right hip":           child.name = "rhip";        break;
                    case "right knee":          child.name = "rknee";       break;
                    case "right foot":          child.name = "rheel";       break;

                    case "left hip":            child.name = "lhip";        break;
                    case "left knee":           child.name = "lknee";       break;
                    case "left foot":           child.name = "lheel";       break;
                }

                RenameModelBones(child.gameObject);
            }
        }

        public static void FlipTextureVertically(Texture2D original)
        {
            var originalPixels = original.GetPixels();

            Color[] newPixels = new Color[originalPixels.Length];

            int width = original.width;
            int rows = original.height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
                }
            }

            original.SetPixels(newPixels);
            original.Apply();
        }

        private static void ExportTextures(Material[] Mats, string OutPath)
        {
            var beforesRGBWrite = GL.sRGBWrite;
            GL.sRGBWrite = true;
            foreach (var mat in Mats)
            {
                var tex = mat.mainTexture as Texture2D;
                FlipTextureVertically(tex);
                var texData = tex.EncodeToPNG();
                var texPath = Path.Combine(OutPath, $"{tex.name}.png");
                File.WriteAllBytes(texPath, texData);
            }
            GL.sRGBWrite = beforesRGBWrite;
        }
    }
}