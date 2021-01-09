using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;

namespace ModTools
{
    public static class FileAcess
    {
        public static List<string> SearchPaths = new List<string>();

        public static List<FileEntry>           FileEntries = new List<FileEntry>();
        public static Dictionary<string, TSPak> LoadedPaks  = new Dictionary<string, TSPak>();

        public static void Init()
        {
            var dataPath = Application.isEditor
                ? Path.Combine(new DirectoryInfo(Application.dataPath).Parent.Parent.FullName, "Data")
                : Path.Combine(Application.dataPath, "Data");

            SearchPaths = new List<string>()
            {
                dataPath
            };


            BuildFileList();
        }

        public static void BuildFileList()
        {
            var fileListBUildSw = Stopwatch.StartNew();

            FileEntries.Clear();

            var topDirs = SearchPaths.SelectMany(Directory.GetDirectories);
            //foreach (var dir in topDirs) {
            Parallel.ForEach(topDirs, dir =>
            {
                var entry = BuildFileListForPath(dir);
                
                // Get the platform and set it on all the sub nodes
                entry.Platform = TryGetPlatformForPath(entry);
                SetDataOnAllChildNodes(entry, Entry => Entry.Platform = entry.Platform );
                
                FileEntries.Add(entry);
            });

            //SortEntries(FileEntries);

            fileListBUildSw.Stop();
            Debug.Log($"Building file list too: {fileListBUildSw.Elapsed.ToString()}");
        }

        private static void SortEntries(List<FileEntry> fileEnties)
        {
            foreach (var fileEntry in fileEnties) {
                if (fileEntry.PakFile == null) {
                    fileEntry.Children = fileEntry.Children.OrderBy(x => (int) x.EntryType).ToList();

                    if (fileEntry.Children.Count > 0) {
                        //fileEntry.Children.Sort((x, y) => (int)x.EntryType - (int)y.EntryType);
                        SortEntries(fileEntry.Children);
                    }
                }
            }
        }

        // Ty to work out the platform based on the directory structure
        // Assume looking at the full disc data and from the root
        public static Platforms TryGetPlatformForPath(FileEntry rootFe)
        {
            // Check for PS2 first
            if (rootFe.Children.Any(x => x.Name.ToLower() == "system.cnf")) {
                // TS1
                if (rootFe.Children.FirstOrDefault(x => x.Name.ToLower()           == "music")?.Children
                          .FirstOrDefault(y => y.Name.ToLower().EndsWith("msc")) != default) {
                    return Platforms.TS1;
                }
                
                // TS2
                if (rootFe.Children.Any(x => x.EntryType == EntryTypes.Dir && x.Name.ToLower() == "trail")) {
                    return Platforms.TS2_PS2;
                }

                // TS3
                if (rootFe.Children.FirstOrDefault(x => x.Name.ToLower() == "pak")?.Children
                          .FirstOrDefault(y => y.Name.ToLower()          == "stream") != default) {
                    return Platforms.TS3_PS2;
                }
            }

            // Gamecube
            if (rootFe.Children.Any(x => x.EntryType == EntryTypes.Dir && x.Name.ToLower() == "sys")) {
                if (rootFe.Children.FirstOrDefault(x => x.Name.ToLower() == "files")?.Children
                          .FirstOrDefault(y => y.Name.ToLower()          == "boss") != default) {
                    return Platforms.TS3_GC;
                }
                else {
                    return Platforms.TS2_GC;
                }
            }
            
            // Xbox
            if (rootFe.Children.Any(x => x.Name.ToLower() == "default.xbe")) {
                if (rootFe.Children.FirstOrDefault(x => x.Name.ToLower() == "data")?.Children
                          .FirstOrDefault(y => y.Name.ToLower()          == "cs_l_38.pak") != default) {
                    return Platforms.TS2_XBOX;
                }
            }

            return Platforms.Unknown;
        }

        private static void SetDataOnAllChildNodes(FileEntry fe, Action<FileEntry> action)
        {
            action(fe);
            
            foreach (var childFe in fe.Children) {
                SetDataOnAllChildNodes(childFe, action);
            }
        }

        public static FileEntry BuildFileListForPath(string DirPath)
        {
            var rootDirInfo = new DirectoryInfo(DirPath);
            var mainFIleEntry = new FileEntry()
            {
                BasePath  = DirPath,
                Name      = rootDirInfo.Name,
                EntryType = EntryTypes.Dir
            };

            var filesAndDirs = rootDirInfo.GetFileSystemInfos();
            foreach (var fsEntry in filesAndDirs) {
                //Parallel.ForEach(filesAndDirs, fsEntry =>
                //{
                if (fsEntry.Attributes.HasFlag(FileAttributes.Directory)) {
                    var subEntrie = BuildFileListForPath(fsEntry.FullName);
                    subEntrie.Parent = mainFIleEntry;
                    mainFIleEntry.Children.Add(subEntrie);
                }
                else {
                    var isAPak = Path.GetExtension(fsEntry.Name)
                                     .Equals(".PAK", StringComparison.InvariantCultureIgnoreCase);
                    var fileE = new FileEntry()
                    {
                        BasePath  = DirPath,
                        Name      = fsEntry.Name,
                        EntryType = isAPak ? EntryTypes.Pak : EntryTypes.File,
                        FileType  = FileTypes.Unknown,
                        Parent    = mainFIleEntry
                    };

                    SetFileTypeForFile(fileE);

                    if (isAPak) {
                        AddPakEntries(fsEntry, fileE);
                    }

                    mainFIleEntry.Children.Add(fileE);
                }
            } //);

            return mainFIleEntry;
        }

        private static void AddPakEntries(FileSystemInfo fsEntry, FileEntry fileE)
        {
            try {
                if (!LoadedPaks.ContainsKey(fsEntry.FullName)) {
                    var pak = new TSPak(fsEntry.FullName);
                    LoadedPaks.Add(fsEntry.FullName, pak);
                }

                if (LoadedPaks.TryGetValue(fsEntry.FullName, out TSPak loadedPak)) {
                    var fileListing = loadedPak.GetFileList();

                    // Create the dir nodes for the paths in the pak
                    var orderedFileList = fileListing.OrderBy(x => x);
                    var uniqueDirs = orderedFileList
                                    .Select(x => x.Contains('/') ? x.Substring(0, x.LastIndexOf('/')) : x)
                                    .Distinct();
                    var createdDirs = new Dictionary<string, FileEntry>();
                    foreach (var dirPath in uniqueDirs) {
                        var dirs     = dirPath.Split('/');
                        var tempPath = "";
                        for (int i = 0; i < dirs.Length; i++) {
                            var dir         = dirs[i];
                            var newTempPath = i == 0 ? dir : $"{tempPath}/{dir}";

                            if (!createdDirs.ContainsKey(newTempPath)) {
                                var parentPak = createdDirs.TryGetValue(tempPath, out FileEntry foundParentNode)
                                    ? foundParentNode
                                    : fileE;
                                var pakDirEntry = new FileEntry()
                                {
                                    BasePath  = i == 0 ? fileE.FullPath : tempPath,
                                    Name      = dir,
                                    EntryType = EntryTypes.Dir,
                                    Parent    = parentPak
                                };

                                parentPak.Children.Add(pakDirEntry);

                                createdDirs.Add(newTempPath, pakDirEntry);
                            }

                            tempPath = newTempPath;
                        }
                    }

                    // Add the files to the dirs
                    foreach (var filePath in fileListing) {
                        var lastPathSepratorIdx = filePath.LastIndexOf('/');
                        var pakFileE = new FileEntry()
                        {
                            BasePath  = filePath.Substring(0, lastPathSepratorIdx),
                            Name      = filePath.Substring(lastPathSepratorIdx + 1),
                            EntryType = EntryTypes.File,
                            PakFile   = fileE
                        };

                        if (createdDirs.TryGetValue(pakFileE.BasePath, out FileEntry pakDirE)) {
                            pakFileE.Parent = pakDirE;
                            pakDirE.Children.Add(pakFileE);
                        }

                        SetFileTypeForFile(pakFileE);
                    }
                }
            }
            catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }

        // Try and work out the file type from the extension, the path or some info in the file
        public static FileTypes SetFileTypeForFile(FileEntry fileEntry)
        {
            // Temp don't bother with ts3 on ps2 untill I add name matching
            if (fileEntry.Platform == Platforms.TS3_PS2) return FileTypes.Unknown;
            fileEntry.FileType = fileEntry.FileType = FileTypes.Unknown;
            
            var ext = Path.GetExtension(fileEntry.Name).ToLower();
            if (ext != ".raw") {
                var typ = ext switch
                {
                    ".mib" => FileTypes.Music,
                    ".vag" => FileTypes.Sfx,
                    ".gct" => FileTypes.Texture,
                    _      => FileTypes.Unknown
                };

                fileEntry.FileType = typ;
                return typ;
            }

            // Check based on file path
            var fileFullPAth  = fileEntry.FullPath;
            if (fileEntry.IsInAPak) {
                var topDirName    = fileFullPAth.Substring(0, fileFullPAth.IndexOf('/'));
                var topDir        = fileEntry.PakFile.Children.FirstOrDefault(x => x.Name == topDirName);
                var fileName      = fileEntry.Name.ToLower();
                var fileNameNoExt = Path.GetFileNameWithoutExtension(fileEntry.Name);
                if (fileName.StartsWith("level") && fileEntry.Parent?.Name.ToLower() == fileNameNoExt) {
                    fileEntry.FileType = FileTypes.Level;
                }
                else if (topDir?.Name.ToLower() == "textures") {
                    fileEntry.FileType = FileTypes.Texture;
                }
                else if (topDir?.Name.ToLower() == "ob") {
                    fileEntry.FileType = FileTypes.Mesh;
                }
                else {
                    fileEntry.FileType = FileTypes.Unknown;
                }
            }

            // load and scan the file
            try {
                if (fileEntry.FileType == FileTypes.Unknown &&
                    LoadedPaks.TryGetValue(fileEntry.PakFile.FullPath, out TSPak pakFile)) {
                    var fileData = pakFile.GetFile(fileEntry.FullPath);
                    if (fileData != null && fileData.Length >= 4) {
                        var fileMagic = new string(new char[]
                            {(char) fileData[0], (char) fileData[1], (char) fileData[2], (char) fileData[3]});
                        var typ = fileMagic switch
                        {
                            "ANR1" => FileTypes.Animation,
                            _      => FileTypes.Unknown
                        };

                        fileEntry.FileType = typ;
                    }
                }
            }
            catch (Exception e) {
                Debug.LogWarning(e.ToString());
            }

            //fileEntry.FileType = FileTypes.Unknown;
            return fileEntry.FileType;
        }
    }

    public class FileEntry
    {
        public EntryTypes EntryType;
        public FileTypes  FileType;
        public Platforms  Platform;
        public string     BasePath;
        public string     Name;

        public FileEntry       PakFile = null; // if this file is in a pak this will refrance the pak its in
        public FileEntry       Parent;
        public List<FileEntry> Children = new List<FileEntry>();

        public string FullPath => PakFile == null ? Path.Combine(BasePath, Name) : $"{BasePath}/{Name}";
        public bool   IsInAPak => PakFile != null;

        // Get a byte array of all th file data
        // Doesn't work on directories
        public byte[] GetFileData()
        {
            if (EntryType == EntryTypes.Dir) return null; // nope for dirs
            
            // in a pak
            if (IsInAPak) {
                if (FileAcess.LoadedPaks.TryGetValue(PakFile.FullPath, out TSPak pak)) {
                    var fileData = pak.GetFile(FullPath);
                    return fileData;
                }
                return null;
            }

            // just a file
            if (File.Exists(FullPath)) {
                var fileData = File.ReadAllBytes(FullPath);
                return fileData;
            }

            return null;
        }
    }

    public struct FileEntryHierarchy
    {
        public int       Parent;
        public List<int> Children;
    }

    public enum EntryTypes : byte
    {
        Dir,
        Pak,
        File
    }

    public enum Platforms : byte
    {
        Unknown,
        TS1,
        TS2_PS2,
        TS2_GC,
        TS2_XBOX,
        TS3_PS2,
        TS3_GC,
        TS3_XBOX
    }

    public enum FileTypes : byte
    {
        Unknown,
        Texture,
        Mesh,
        Sfx,
        Music,
        Pad,
        Level,
        Animation
    }
}