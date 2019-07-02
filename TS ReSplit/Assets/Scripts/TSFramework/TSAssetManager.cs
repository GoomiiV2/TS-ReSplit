using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TSAssetManager
{
    public static string RunTimeDataPath = $"{Application.dataPath}../../../Data"; // Where the orignal game content is located
    private static Dictionary<string, TSPak> PakFiles = new Dictionary<string, TSPak>();
    private static MediaSource MediaTypeSource = MediaSource.Files;

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

    #region Internals
    private static byte[] LoadFileFromDisk(string Filepath)
    {
        var data = File.ReadAllBytes(Filepath);
        return data;
    }

    public static bool IsPakLoaded(string PakFilePath)
    {
        var isLoaded = PakFiles.ContainsKey(PakFilePath);
        return isLoaded;
    }

    private static string GetPakPath(string PakPath)
    {
        var pathToPak = Path.Combine(RunTimeDataPath, PakPath);
        return pathToPak;
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
            var path  = Path.Combine(RunTimeDataPath, Dir);
            var files = Directory.GetFiles(path, Pattern, SearchOption.AllDirectories);
            return files;
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
