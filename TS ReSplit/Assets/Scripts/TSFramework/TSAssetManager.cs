using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class TSAssetManager
{
    public static readonly List<(TSGame Game, string[] PS2DiscIDs)> PS2GameIDsMapping = new List<(TSGame Game, string[] PS2DiscIDs)>()
    {
        (TSGame.TimeSplitters2, new string[] { "SLES_50877", "SLUS_20314", "SLPS_25207", "SLKA_25020" })
    };

    public static readonly Dictionary<string, TSGame> GameIDMapping = new Dictionary<string, TSGame>()
    {
        { "TS1", TSGame.TimeSplitters1 },
        { "TS2", TSGame.TimeSplitters2 },
        { "TS3", TSGame.TimeSplitters3 }
    };

    public static string RunTimeDataPath = ""; // Where the orignal game content is located

    // TODO: Add some flush levels so that when level paks can get unloaded from memory when a new level is loaded and such
    private static Dictionary<string, TSPak> PakFiles = new Dictionary<string, TSPak>();
    private static MediaSource MediaTypeSource        = MediaSource.Files;
    private static TSGame GameType                    = TSGame.TimeSplitters2;
    private static string DVDDrivePath                = "";

    static TSAssetManager()
    {
        Init();
    }

    // Decide what media to load the game content from
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        // Check for a dvd disc
        var disc = FindDriveWithGameDisc();
        if (disc != null)
        {
            MediaTypeSource = MediaSource.Disc;
            DVDDrivePath    = disc.Value.Drive;
        }
        else
        {
            MediaTypeSource = MediaSource.Files;
            if (Application.isEditor)
            {
                RunTimeDataPath = $"{Application.dataPath}../../../Data";
            }
            else
            {
                RunTimeDataPath = Path.Combine(Application.dataPath, "Data");
            }
        }

        Debug.Log($"TSAssetManager::Init MediaTypeSource: {MediaTypeSource}");
    }

    public static byte[] LoadFile(string FilePath)
    {
        var pakPath = GetPakForPath(FilePath);

        if (pakPath == null)
        {
            var data = LoadFileFromDisk(FilePath);
            return data;
        }
        else
        {
            var data = LoadFileFromPak(pakPath.Item1, pakPath.Item2);
            return data;
        }
    }

    public static byte[] LoadFileFromPak(string Pak, string FileInPak)
    {
        TSPak pakFile;
        bool pakIsLoaded = PakFiles.TryGetValue(Pak, out pakFile);
        byte[] data;

        if (pakIsLoaded)
        {
            data = pakFile.GetFile(FileInPak);
        }
        else
        {
            pakFile = new TSPak();
            pakFile.LoadEntries(GetPakPath(Pak));
            PakFiles.Add(Pak, pakFile);

            Debug.Log($"Pak {Pak} wasn't loaded so loaded it now");

            data = pakFile.GetFile(FileInPak);
        }

        if (data == null)
        {
            Debug.LogWarning($"FIle ({FileInPak}) wasn't found in pak: {Pak}");
        }

        return data;
    }

    public static List<string> GetFileListForPak(string PakPath)
    {
        // To load the pak, hacky
        LoadFileFromPak(PakPath, "");

        var pakEntries = PakFiles[PakPath].GetFileList();
        return pakEntries;
    }

    public static string GetCurrentDataPath()
    {
        if (MediaTypeSource == MediaSource.Disc)
        {
            return DVDDrivePath;
        }
        else if (MediaTypeSource == MediaSource.Files)
        {
            return RunTimeDataPath;
        }
        else
        {
            return RunTimeDataPath;
        }
    }

    #region Internals
    private static byte[] LoadFileFromDisk(string Filepath)
    {
        var gameIDStr   = Filepath.Substring(0, 3).ToUpper();
        var hasGameType = GameIDMapping.TryGetValue(gameIDStr, out TSGame GameID);

        if (MediaTypeSource == MediaSource.Disc)
        {
            var pak = hasGameType ? Filepath.Substring(4, Filepath.Length - 4) : Filepath;

            var path = Path.Combine(DVDDrivePath, pak);
            var data      = File.ReadAllBytes(path);
            return data;
        }
        else
        {
            var path      = Path.Combine(RunTimeDataPath, Filepath);
            var data      = File.ReadAllBytes(path);
            return data;
        }
    }

    public static bool IsPakLoaded(string PakFilePath)
    {
        var isLoaded = PakFiles.ContainsKey(PakFilePath);
        return isLoaded;
    }

    private static string GetPakPath(string PakPath)
    {
        var gameIDStr   = PakPath.Substring(0, 3).ToUpper();
        var hasGameType = GameIDMapping.TryGetValue(gameIDStr, out TSGame GameID);

        if (MediaTypeSource == MediaSource.Disc)
        {
            // Remove the "ts2/" prefix for now if loading from a dvd, only game supported
            var pak = hasGameType ? PakPath.Substring(4, PakPath.Length - 4) : PakPath;

            var pathToPak = Path.Combine(DVDDrivePath, pak);
            return pathToPak;
        }
        else
        {
            var pathToPak = Path.Combine(RunTimeDataPath, PakPath);
            return pathToPak;
        }
    }

    // If the given path is for a file in a pak, returns a ref to the pak that contains that file
    // returns null if its not a path for a file in a pak
    public static Tuple<string, string> GetPakForPath(string Filepath)
    {
        const string PAK_EXTENSION = ".PAK";
        if (Filepath != null || Filepath != "")
        {
            int endOfPakPath = Filepath.ToUpperInvariant().LastIndexOf(PAK_EXTENSION);
            if (endOfPakPath > 0)
            {
                string pakPath       = Filepath.Substring(0, endOfPakPath + PAK_EXTENSION.Length);
                string fileInPakPath = Filepath.Replace(pakPath, "").TrimStart(new char[] { '/', '\\' });
                return Tuple.Create(pakPath, fileInPakPath);
            }
        }

        return null;
    }

    // Abstarct this so the media doesn't matter
    public static string[] GetFilesInDir(string Dir, string Pattern)
    {
        if (MediaTypeSource == MediaSource.Files)
        {
            var basePath = GetCurrentDataPath();
            var path     = Path.Combine(basePath, Dir);
            var files    = Directory.GetFiles(path, Pattern, SearchOption.AllDirectories);
            return files;
        }
        else if (MediaTypeSource == MediaSource.Disc)
        {
            var basePath = GetCurrentDataPath();
            var pak      = Dir.Substring(4, Dir.Length - 4);
            var path     = Path.Combine(basePath, pak);
            var files    = Directory.GetFiles(path, Pattern, SearchOption.AllDirectories);
            return files;
        }

        return null;
    }

    // Scan the attached DVD drives for one with a TimeSplitters disc in it
    public static (string Drive, TSGame Game)? FindDriveWithGameDisc()
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType == DriveType.CDRom && drive.IsReady)
            {
                var ps2GameID = GetPS2GameIDFromDisc(drive.RootDirectory.ToString());
                var gameType  = PS2GameIDsMapping.Where(x => x.PS2DiscIDs.Any(y => y.Equals(ps2GameID, StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault();
                if (gameType != default)
                {
                    return (drive.RootDirectory.ToString(), gameType.Game);
                }
            }
        }

        return null;
    }

    // Why not just use the volumelabel? because .Net doesn't give me that :<
    // So parse the SYSTEM.CNF file on the disc and get the id from that
    public static string GetPS2GameIDFromDisc(string DrivePath)
    {
        var filePath = Path.Combine(DrivePath, "SYSTEM.CNF");
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var kvp = line.Split(new char[] { '=' });
                if (kvp[0].Trim().Equals("BOOT2", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = kvp[1].Trim().ToLower().Replace(@"cdrom0:\", "");
                    id     = id.Substring(0, id.Length - 2).Replace(".", "");
                    return id;
                }
            }
        }

        return null;
    }

    #endregion
}

public struct PakFileData
{
    public string PakPath;
    public Dictionary<string, TSPakEntry> FilesData;
    public FileStream FileHandle;
}

public enum TSGame
{
    TimeSplitters1,
    TimeSplitters2,
    TimeSplitters3
}

public enum MediaSource
{
    Files,
    Iso,
    Disc
}
