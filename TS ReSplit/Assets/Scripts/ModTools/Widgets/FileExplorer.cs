using IconFonts;
using ImGuiNET;

namespace ModTools.Widgets
{
    public class FileExplorer : BaseWidget
    {
        public override string WidgetName => "File Explorer";
        
        public FileExplorer(FileEntry fe = null) : base(fe)
        {
            BuildFileList();
        }

        public void BuildFileList()
        {
            FileAcess.BuildFileList();
        }

        public override void Draw()
        {
            if (ImGui.Begin(WidgetName, ImGuiWindowFlags.NoCollapse)) {
                foreach (var fe in FileAcess.FileEntries) {
                    DrawFileListingNode(fe);
                }

                ImGui.End();
            }
        }

        private void DrawFileListingNode(FileEntry fe)
        {
            if (fe.EntryType != EntryTypes.File) {
                if (DrawDirOrPakEntry(fe)) {
                    foreach (var cFe in fe.Children) {
                        DrawFileListingNode(cFe);
                    }

                    ImGui.TreePop();
                }
            }
            else {
                DrawFileEntry(fe);
            }
        }

        private bool DrawDirOrPakEntry(FileEntry fe)
        {
            string platformIcon = fe.Platform switch
            {
                // Playstation
                Platforms.TS1     => "\uf3df",
                Platforms.TS2_PS2 => "\uf3df",
                Platforms.TS3_PS2 => "\uf3df",

                // Xbox
                Platforms.TS2_XBOX => "\uf412",
                Platforms.TS3_XBOX => "\uf412",

                // Gamecube
                Platforms.TS2_GC => "\uf1b2",
                Platforms.TS3_GC => "\uf1b2",
                
                _ => null
            };

            var startPos = ImGui.GetCursorPosX();
            ImGui.SetCursorPosX(startPos + 20);
            var nodeExpanded = ImGui.TreeNode(fe.Name);
            DrawFileEntryContextMenu(fe);
            
            ImGui.SameLine();
            ImGui.SetCursorPosX(startPos); 
            ImGui.Text(fe.EntryType == EntryTypes.Dir ? FontAwesome5.Folder : FontAwesome5.FileArchive);

            return nodeExpanded;
        }

        private void DrawFileEntry(FileEntry fe)
        {
            string fileIcon = fe.FileType switch
            {
                FileTypes.Unknown   => FontAwesome5.Question,
                FileTypes.Sfx       => FontAwesome5.FileAudio,
                FileTypes.Music     => FontAwesome5.Music,
                FileTypes.Level     => FontAwesome5.Map,
                FileTypes.Texture   => FontAwesome5.PhotoVideo,
                FileTypes.Animation => FontAwesome5.Walking,
                FileTypes.Pad       => FontAwesome5.MapSigns,
                FileTypes.Mesh      => FontAwesome5.Shapes
            };


            ImGui.PushID(fe.GetHashCode());
            var isSelected = ImGui.Selectable("##", false, ImGuiSelectableFlags.AllowItemOverlap);
            ImGui.SameLine();

            var isDoubleClicked = ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
            //ImGui.SameLine();
            
            ImGui.Text(fileIcon);
            ImGui.SameLine();
            ImGui.Text(fe.Name);
            
            // List view extensions
            var feListExtHash = FileActionAttribute.MakeHash(fe.Platform, fe.FileType, fe.EntryType, FileActionAttribute.ActionTypes.ListDisplay);
            if (ModToolsMain.ModTools.FileActionsLookup.TryGetValue(feListExtHash, out ModToolsMain.FileActionDelegate feListAction)) {
                feListAction(fe);
            }

            DrawFileEntryContextMenu(fe);

            // Double click action
            if (isDoubleClicked) {
                var feHash = FileActionAttribute.MakeHash(fe.Platform, fe.FileType, fe.EntryType,
                    FileActionAttribute.ActionTypes.DoubleClick);
                
                if (ModToolsMain.ModTools.FileActionsLookup.TryGetValue(feHash, out ModToolsMain.FileActionDelegate feAction)) {
                    feAction(fe);
                }
            }
            
            ImGui.PopID();
        }

        private void DrawFileEntryContextMenu(FileEntry fe)
        {
            //ImGui.PushID(fe.GetHashCode());
            if (ImGui.BeginPopupContextItem("##")) {
                
                // File actions
                var feHash = FileActionAttribute.MakeHash(fe.Platform, fe.FileType, fe.EntryType,
                    FileActionAttribute.ActionTypes.ContextMenuExtension);

                if (ModToolsMain.ModTools.FileActionsLookup.TryGetValue(feHash, out ModToolsMain.FileActionDelegate feAction)) {
                    feAction(fe);
                }
                
            #region Debug Info

                ImGui.Text($"FullPath: {fe.FullPath}");
                ImGui.Text($"EntryType: {fe.EntryType}");
                ImGui.Text($"FileType: {fe.FileType}");
                ImGui.Text($"Platform: {fe.Platform}");
                if (fe.PakFile != null) {
                    ImGui.Text($"PakfIle Path: {fe.PakFile.FullPath}");
                }

                ImGui.Separator();

            #endregion


                ImGui.EndPopup();
            }

            //ImGui.PopID();
        }
    }
}