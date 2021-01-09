using System;
using System.Linq;
using ImGuiNET;
using UnityEngine;

namespace ModTools.Widgets
{
    public class ThreeDViewport : BaseWidget
    {
        public static int NumOpen3DViews = 0;
        
        public GameObject GetScenePrefab => ModToolsMain.ModTools.ModelViewerPrefab;

        public Camera        SceneCamera;
        public RenderTexture ViewportRT;
        public GameObject    ScenePrefabInst;

        public ThreeDViewport(FileEntry fe) : base(fe)
        {
            // Hack for not spawning two viewports for the same model
            if (ModToolsMain.ModTools.Widgets.Any(x => x.WidgetName == WidgetName && !x.ShouldClose)) {
                ShouldClose = true;
                return;
            }
            
            FileEntryForWidget = fe;
            CreateScene();
        }

        public virtual void CreateScene()
        {
            if (ScenePrefabInst != null) return;
            NumOpen3DViews++;
            
            ScenePrefabInst                    = GameObject.Instantiate(GetScenePrefab);
            ScenePrefabInst.transform.position = new Vector3(0, 100 * NumOpen3DViews, 0);
            ScenePrefabInst.name               = $"Model Viewer Setup {FileEntryForWidget.FullPath}";
            SceneCamera                        = ScenePrefabInst.GetComponentInChildren<Camera>();
            
            Debug.Log($"Creating scene prefab setup {WidgetUuid} ({WidgetName})");

            SceneCamera.cullingMask = (1 << NumOpen3DViews);
            SetLayer(ScenePrefabInst, NumOpen3DViews);

            ViewportRT                = new RenderTexture(640, 480, 1);
            SceneCamera.targetTexture = ViewportRT;
        }

        private void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++) {
                var subGo = go.transform.GetChild(i);
                SetLayer(subGo.gameObject, layer);
            }
        }

        public override void Draw()
        {
            if (ImGui.Begin(WidgetName,
                ref ShouldClose)) {
                //ImGui.BeginMenuBar();

                //ImGui.EndMenuBar();

                var winContentSize = ImGui.GetContentRegionAvail();
                if (winContentSize.x != ViewportRT.width || winContentSize.y != ViewportRT.height) {
                    if (ViewportRT != null) {
                        ViewportRT.Release();
                    }

                    ViewportRT                = new RenderTexture((int) winContentSize.x, (int) winContentSize.y, 1);
                    SceneCamera.targetTexture = ViewportRT;
                }
                
                ImGuiUn.Image(ViewportRT, new Vector2(ViewportRT.width, ViewportRT.height));
            }


            ImGui.End();
        }

        ~ThreeDViewport()
        {
            NumOpen3DViews--;
        }
    }
}