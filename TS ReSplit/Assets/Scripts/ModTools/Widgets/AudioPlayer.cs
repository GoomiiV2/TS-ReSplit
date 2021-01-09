using Assets.Scripts.TSFramework.Singletons;
using IconFonts;
using ImGuiNET;
using UnityEngine;

namespace ModTools.Widgets
{
    public class AudioPlayer : BaseWidget
    {
        public static ListPreviewState PreviewState;
        
        public AudioPlayer(FileEntry fileEntryForWidget) : base(fileEntryForWidget)
        {
        }
        
        [FileActionAttribute(Platforms.TS2_PS2, FileTypes.Sfx, EntryTypes.File, FileActionAttribute.ActionTypes.ListDisplay)]
        public static bool ListPreviewWidget(FileEntry fe)
        {
            ImGui.SameLine();

            var feHash = fe.FullPath.GetHashCode();
            if (PreviewState.CurrentFEHash != feHash || (PreviewState.CurrentFEHash == feHash && !ModToolsMain.AudioPreviewSource.isPlaying)) {
                if (ImGui.Button(FontAwesome5.Play)) {
                    PlayPausePreview(fe);
                    PreviewState.CurrentFEHash = feHash;
                }
            }
            else {
                if (ImGui.Button(FontAwesome5.Pause)) {
                    PlayPausePreview(fe, true);
                }
            }

            return true;
        }

        [FileActionAttribute(Platforms.TS1, FileTypes.Sfx, EntryTypes.File,
            FileActionAttribute.ActionTypes.ListDisplay)]
        public static bool ListPreviewWidgetTS1(FileEntry fe)
        {
            return ListPreviewWidget(fe);
        }
        
        [FileActionAttribute(Platforms.TS3_PS2, FileTypes.Sfx, EntryTypes.File,
            FileActionAttribute.ActionTypes.ListDisplay)]
        public static bool ListPreviewWidgetTS3(FileEntry fe)
        {
            return ListPreviewWidget(fe);
        }
        
        [FileActionAttribute(Platforms.TS2_PS2, FileTypes.Music, EntryTypes.File,
            FileActionAttribute.ActionTypes.ListDisplay)]
        public static bool ListPreviewWidgetTS3Music(FileEntry fe)
        {
            return ListPreviewWidget(fe);
        }
        
        [FileActionAttribute(Platforms.TS3_PS2, FileTypes.Music, EntryTypes.File,
            FileActionAttribute.ActionTypes.ListDisplay)]
        public static bool ListPreviewWidgetTS2Music(FileEntry fe)
        {
            return ListPreviewWidget(fe);
        }

        public static void PlayPausePreview(FileEntry fe, bool newClip = false)
        {
            if (newClip) {
                if (ModToolsMain.AudioPreviewSource.isPlaying) {
                    ModToolsMain.AudioPreviewSource.Pause();
                }
                else {
                    ModToolsMain.AudioPreviewSource.Play();
                }
            }
            else {
                if (fe.FileType == FileTypes.Music && fe.Platform == Platforms.TS1 || fe.Platform == Platforms.TS2_PS2 || fe.Platform == Platforms.TS3_PS2) {
                    PreviewState.MibFile = new StreamedMib(fe.GetFileData());
                    PreviewState.Clip    = PreviewState.MibFile.Clip;
                }
                else {
                    PreviewState.Clip = ReSplit.Audio.GetAudioClip(fe.GetFileData(), fe.Name);
                }
                
                ModToolsMain.AudioPreviewSource.clip = PreviewState.Clip;

                ModToolsMain.AudioPreviewSource.Play();
            }
        }

        public struct ListPreviewState
        {
            public int       CurrentFEHash;
            public FileEntry CurrentFE;

            public StreamedMib MibFile;
            public AudioClip   Clip;
        }
    }
}