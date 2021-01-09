using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ImGuiNET;
using ModTools.Widgets;
using UnityEngine.Diagnostics;
using UnityEngine.Windows.Speech;
using Utils = Assets.Scripts.TSFramework.Utils;

namespace ModTools
{
    //[ExecuteInEditMode]
    public class ModToolsMain : MonoBehaviour
    {
        public static ModToolsMain ModTools;
        public static AudioSource  AudioPreviewSource;
        
        public GameObject   ModelViewerPrefab;

        public  List<BaseWidget> Widgets       = new List<BaseWidget>();
        private List<BaseWidget> WidgetsToOpen = new List<BaseWidget>();

        public delegate bool FileActionDelegate(FileEntry fe);

        public Dictionary<int, FileActionDelegate> FileActionsLookup = new Dictionary<int, FileActionDelegate>();

        public void Start()
        {
            ModTools           = this;
            AudioPreviewSource = GetComponent<AudioSource>();
            
            GetFileActions();

            FileAcess.Init();

            Widgets.Clear();

            // Add the default widgets
            Widgets.Add(new FileExplorer());
        }

        // Reflection get FileAction attributes
        private void GetFileActions()
        {
            var fileActions = Assembly.GetExecutingAssembly().GetTypes()
                                      .SelectMany(t => t.GetMethods())
                                      .Where(m => m.GetCustomAttributes(typeof(FileActionAttribute), false).Length > 0);

            foreach (var fileAction in fileActions) {
                var attrib = fileAction.GetCustomAttribute<FileActionAttribute>();
                var hashId = FileActionAttribute.MakeHash(attrib.Platform, attrib.FileType, attrib.EntryType,
                    attrib.ActionType);
                var dele = (FileActionDelegate)Delegate.CreateDelegate(typeof(FileActionDelegate), null, fileAction);

                FileActionsLookup.Add(hashId, dele);
            }
        }

        void OnEnable()
        {
            ImGuiUn.Layout += OnLayout;
            //Application.targetFrameRate = 60;
            Debug.Log("ModToolsMain:OnEnable");
        }

        void OnDisable()
        {
            ImGuiUn.Layout -= OnLayout;
            //Application.targetFrameRate = -1;
            Debug.Log("ModToolsMain:OnDisable");
        }

        void OnLayout()
        {
            //ImGui.ShowDemoWindow();


            DrawMenuBar();
            var menuBarHeight = new Vector2(0, ImGui.GetWindowHeight());
            var viewport      = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(menuBarHeight);
            ImGui.SetNextWindowSize(viewport.Size - menuBarHeight);
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(1.0f, 1.0f));
            ;
            if (ImGui.Begin("##MainArea",
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoBringToFrontOnFocus)) {
                var dockId = ImGui.GetID("MainDockSPace");
                ImGui.DockSpace(dockId, Vector2.zero, ImGuiDockNodeFlags.PassthruCentralNode);
                ImGui.PopStyleVar(3);

                // Draw and mage widgets
                ManageWidgets();
            }

            ImGui.End();

            ImGui.EndMainMenuBar();
        }

        public void OpenWidget(BaseWidget widget)
        {
            WidgetsToOpen.Add(widget);
        }

        private void ManageWidgets()
        {
            var widgetsToClose = new List<int>();

            foreach (var widget in Widgets) {
                if (widget.ShouldClose) {
                    widgetsToClose.Add(widget.WidgetUuid);
                }
                else {
                    widget.Draw();
                }
            }

            Widgets.RemoveAll(x => x.ShouldClose);

            /*foreach (var widget in widgetsToClose) {
                Widgets.RemoveAll(widget);
            }*/

            foreach (var widget in WidgetsToOpen) {
                Widgets.Add(widget);
            }

            WidgetsToOpen.Clear();
        }

        void DrawMenuBar()
        {
            ImGui.BeginMainMenuBar();

            // Menu bar logo and version
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 110);
            ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xDE3163FF), "Re");
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 8);
            ImGui.Text("Split");
            ImGui.SameLine();
            ImGui.Text(Utils.Version);
        }
    }
}