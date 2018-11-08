using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

// I miss Spans already :<
// Due a refactor
public class FPPak
{
    private byte[] Data = null;
    private FPPPakHeader Header;
    public List<FPPakFile> Files = null;

    public FPPak() { }

    public FPPak(string FilePath)
    {
        Load(FilePath);
    }

    public void Load(string FilePath)
    {
        using (var file = File.Open(FilePath, FileMode.Open))
        {
            if (file != null)
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    LoadHeader(reader);

                    var headerMagic = new string(Header.Magic);
                    if (headerMagic == "P5CK") // Timesplitters 3: Future Perfect
                    {
                        LoadPakV5(reader);
                    }
                    else if (headerMagic == "P4CK") // Timesplitters 2
                    {
                        LoadPakV4(reader);
                    }
                }
            }
        }
    }

    public TSPakEntry[] LoadEntries(string FilePath)
    {
        using (var file = File.Open(FilePath, FileMode.Open))
        {
            if (file != null)
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    LoadHeader(reader);

                    var headerMagic = new string(Header.Magic);
                    if (headerMagic == "P5CK") // Timesplitters 3: Future Perfect
                    {
                        var dirEntrys = LoadDirEntriesV5(reader);
                        var entries = dirEntrys.Select(x => new TSPakEntry()
                        {
                            Name   = $"{x.ID}",
                            Offset = x.Offset,
                            Size   = x.Length
                        }).ToArray();

                        return entries;
                    }
                    else if (headerMagic == "P4CK") // Timesplitters 2
                    {
                        var dirEntrys = LoadDirEntriesV4(reader);
                        var entries = dirEntrys.Select(x => new TSPakEntry()
                        {
                            Name   = x.Name,
                            Offset = x.Offset,
                            Size   = x.Length
                        }).ToArray();

                        return entries;
                    }
                }
            }
        }

        return null;
    }

    private void LoadHeader(BinaryReader R)
    {
        Header.Magic     = R.ReadChars(4);
        Header.DirOffset = R.ReadUInt32();
        Header.DirSize   = R.ReadUInt32();
    }

    private void LoadPakV4(BinaryReader R)
    {
        var dirEntrys = LoadDirEntriesV4(R);
        Files         = LoadFilesV4(R, dirEntrys);
    }

    private void LoadPakV5(BinaryReader R)
    {
        var dirEntrys = LoadDirEntriesV5(R);
        Files         = LoadFilesV5(R, dirEntrys);
    }

    #region Pak version 5
    private FPPakDirEntryV5[] LoadDirEntriesV5(BinaryReader R)
    {
        var numEntrys = Header.DirSize / Marshal.SizeOf(typeof(FPPakDirEntryV5));
        var dirEntrys = new FPPakDirEntryV5[numEntrys];

        R.BaseStream.Seek(Header.DirOffset, SeekOrigin.Begin);

        for (int i = 0; i < numEntrys; i++)
        {
            dirEntrys[i] = ReadDirEntryV5(R);
        }

        return dirEntrys;
    }

    private FPPakDirEntryV5 ReadDirEntryV5(BinaryReader R)
    {
        var entry = new FPPakDirEntryV5()
        {
            ID     = R.ReadUInt32(),
            Offset = R.ReadUInt32(),
            Length = R.ReadUInt32(),
            _Unk   = R.ReadUInt32()
        };

        return entry;
    }

    private List<FPPakFile> LoadFilesV5(BinaryReader R, FPPakDirEntryV5[] DirEntries)
    {
        var files = new List<FPPakFile>(DirEntries.Length);

        foreach (var entry in DirEntries)
        {
            R.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

            var pakFile = new FPPakFile()
            {
                ID   = entry.ID,
                Data = R.ReadBytes((int)entry.Length)
            };

            files.Add(pakFile);
        }

        return files;
    }
    #endregion

    #region Pak version 4
    private FPPakDirEntryV4[] LoadDirEntriesV4(BinaryReader R)
    {
        const int DirEntrySize = 60;
        var numEntrys          = Header.DirSize / DirEntrySize;
        var dirEntrys          = new FPPakDirEntryV4[numEntrys];

        R.BaseStream.Seek(Header.DirOffset, SeekOrigin.Begin);

        for (int i = 0; i < numEntrys; i++)
        {
            dirEntrys[i] = ReadDirEntryV4(R);
        }

        return dirEntrys;
    }

    private FPPakDirEntryV4 ReadDirEntryV4(BinaryReader R)
    {
        const int FILENAME_LENGTH = 48;
        var entry = new FPPakDirEntryV4()
        {
            Name   = new string(R.ReadChars(FILENAME_LENGTH)).TrimEnd(),
            Offset = R.ReadUInt32(),
            Length = R.ReadUInt32(),
            _Unk   = R.ReadUInt32()
        };

        return entry;
    }

    private List<FPPakFile> LoadFilesV4(BinaryReader R, FPPakDirEntryV4[] DirEntries)
    {
        var files = new List<FPPakFile>(DirEntries.Length);

        foreach (var entry in DirEntries)
        {
            R.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

            var pakFile = new FPPakFile()
            {
                Name   = entry.Name,
                Data = R.ReadBytes((int)entry.Length)
            };

            files.Add(pakFile);
        }

        return files;
    }
    #endregion

    public void WriteFilesToDIsk(string Filepath, string NamePrefix = "")
    {
        if (Files == null) { return; }

        foreach (var pakFile in Files)
        {
            var name     = pakFile.Name != null ? pakFile.Name : $"{pakFile.ID}";
            var fullName = $"{NamePrefix}{name}";
            fullName     = Path.GetInvalidPathChars().Aggregate(fullName, (current, c) => current.Replace(c.ToString(), string.Empty));
            var path     = Path.Combine(Filepath, fullName);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, pakFile.Data);
        }
    }
}

public struct FPPPakHeader
{
    public char[] Magic;
    public uint DirOffset;
    public uint DirSize;
}

public struct FPPakDirEntryV5
{
    public uint ID;
    public uint Offset;
    public uint Length;
    public uint _Unk;
}

public struct FPPakDirEntryV4
{
    public string   Name;
    public uint     Offset;
    public uint     Length;
    public uint     _Unk;
}

public class FPPakFile
{
    public uint ID;
    public string Name;
    public byte[] Data;
}
