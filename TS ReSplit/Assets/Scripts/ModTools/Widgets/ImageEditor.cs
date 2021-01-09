using System;
using System.ComponentModel;
using Assets.Scripts.TSFramework;
using DefaultNamespace;
using IconFonts;
using ImGuiNET;
using UnityEngine;

namespace ModTools.Widgets
{
    public class ImageEditor : BaseWidget
    {
        public Platforms Platfom;
        public Texture2D UTexture2D;

        public TS2.Texture TS2Texture;
        public float       Scale                 = 1.0f;
        public bool        ViewBilinnear         = true;
        public bool        TestShouldCLoseWidget = false;

        public override string WidgetName =>
            $"Image Editor: {FileEntryForWidget.Name}##{FileEntryForWidget.FullPath}-{WidgetUuid}";

        public ImageEditor(FileEntry fileEntry) : base(fileEntry)
        {
            Platfom    = Platforms.TS2_PS2;
            TS2Texture = new TS2.Texture(fileEntry.GetFileData());
            UTexture2D = TSTextureUtils.TS2TexToT2D(TS2Texture);
        }

        [FileActionAttribute("OpenPS2PS2Image", Platforms.TS2_PS2, FileTypes.Texture, EntryTypes.File,
            FileActionAttribute.ActionTypes.DoubleClick)]
        public static bool OpenPS2PS2Image(FileEntry fe)
        {
            var result = OpenPlatformImage(Platforms.TS2_PS2, fe);
            return result;
        }

        [FileActionAttribute("OpenPS2PS2ImageContextMenuItem", Platforms.TS2_PS2, FileTypes.Texture, EntryTypes.File,
            FileActionAttribute.ActionTypes.ContextMenuExtension)]
        public static bool OpenPS2PS2ImageContextMenu(FileEntry fe)
        {
            if (DrawContextMenuItem()) {
                var result = OpenPlatformImage(Platforms.TS2_PS2, fe);
                return result;
            }

            return true;
        }

        public static bool DrawContextMenuItem()
        {
            var wasClicked = ImGui.MenuItem($"{FontAwesome5.Image} Open Image");
            return wasClicked;
        }

        public static bool OpenPlatformImage(Platforms platform, FileEntry fe)
        {
            switch (platform) {
                case Platforms.Unknown:
                    return false;
                case Platforms.TS1:
                    return false;
                case Platforms.TS2_PS2:
                {
                    var imgEditor = new ImageEditor(fe);
                    imgEditor.TS2Texture = new TS2.Texture(fe.GetFileData());
                    imgEditor.UTexture2D = TSTextureUtils.TS2TexToT2D(imgEditor.TS2Texture);
                    ModToolsMain.ModTools.OpenWidget(imgEditor);
                    return true;
                }
                case Platforms.TS2_GC:
                    return false;
                case Platforms.TS2_XBOX:
                    return false;
                case Platforms.TS3_PS2:
                    return false;
                case Platforms.TS3_GC:
                    return false;
                case Platforms.TS3_XBOX:
                    return false;
                default:
                    return false;
            }
        }

        public override void Draw()
        {
            if (ImGui.Begin(WidgetName,
                ref TestShouldCLoseWidget, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.MenuBar)) {

                ImGui.BeginMenuBar();
                ImGui.MenuItem("Export");

                ImGui.Text("Zoom: ");
                ImGui.SameLine();
                ImGui.PushItemWidth(ImGui.GetColumnWidth() - 150);
                ImGui.SliderFloat("##Zoom", ref Scale, 1.0f, 10.0f);

                ImGui.SameLine();
                ImGui.Text("Filtering: ");
                ImGui.SameLine();
                ImGui.Checkbox("##Filtering", ref ViewBilinnear);
                UTexture2D.filterMode = ViewBilinnear ? FilterMode.Bilinear : FilterMode.Point;
                
                ImGui.SameLine();
                if (ImGui.Button("Close")) ShouldClose = true;
                
                ImGui.EndMenuBar();


                if (Platfom == Platforms.TS2_PS2) {
                    ImGui.PushItemWidth(40);

                    ImGui.Text("Width: ");
                    ImGui.SameLine();
                    ImGui.InputInt("##Width", ref TS2Texture.Width, 0);
                    ImGui.SameLine();

                    ImGui.Text("Height: ");
                    ImGui.SameLine();
                    ImGui.InputInt("##Height", ref TS2Texture.Height, 0);
                    ImGui.SameLine();

                    ImGui.Text("StrechX: ");
                    ImGui.SameLine();
                    ImGui.InputInt("##StrechX", ref TS2Texture.ID, 0);
                    ImGui.SameLine();

                    ImGui.Text("StrechY: ");
                    ImGui.SameLine();
                    ImGui.InputInt("##StrechY", ref TS2Texture.UNK, 0);
                }

                ImGui.PopItemWidth();

                if (UTexture2D != null) {
                    ImGuiUn.Image(UTexture2D, new Vector2(UTexture2D.width, UTexture2D.height) * Scale);
                }
            }

            ImGui.End();
        }
    }
}