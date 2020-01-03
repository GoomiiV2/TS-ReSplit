using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileNameLookups = System.Collections.Generic.Dictionary<uint, string>;

public class TSPak
{
    public TSPakHeader Header;
    private Dictionary<string, TSPakEntry> FileEntries = new Dictionary<string, TSPakEntry>();
    private string PakFilePath;

    public TSPak()
    {

    }

    public TSPak(string FilePath)
    {
        LoadEntries(FilePath);
    }

    public void LoadEntries(string FilePath)
    {
        PakFilePath = FilePath;

        using (var file = File.OpenRead(FilePath))
        {
            if (file != null)
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    LoadHeader(reader);

                    var pakVersion = GetPakVersion(new string(Header.Magic));
                    switch(pakVersion)
                    {
                        case PakVersion.P4CK: // Timesplitters 2
                            LoadDirEntriesV4(reader);
                            break;

                        case PakVersion.P5CK: // Timesplitters 3: Future Perfect
                            LoadDirEntriesV5(reader);
                            break;

                        case PakVersion.P8CK: // Timesplitters 2 GC and the homefront easter egg for Timesplitters 2
                            LoadDirEntriesV8(reader);
                            break;
                    }
                }
            }
        }
    }

    public byte[] GetFile(string FilePath)
    {
        TSPakEntry fileEntry;
        if (FileEntries.TryGetValue(FilePath, out fileEntry))
        {
            using (var file = File.OpenRead(PakFilePath))
            {
                file.Seek((int)fileEntry.Offset, SeekOrigin.Begin);
                var data = new byte[fileEntry.Size];
                file.Read(data, 0, (int)fileEntry.Size);

                return data;
            }
        }

        return null;
    }

    public List<string> GetFileList()
    {
        var fileNames = new List<string>();
        foreach (var key in FileEntries.Keys)
        {
            fileNames.Add(key);
        }

        return fileNames;
    }

    private void LoadHeader(BinaryReader R)
    {
        Header.Magic           = R.ReadChars(4);
        Header.DirOffset       = R.ReadUInt32();
        Header.DirSize         = R.ReadUInt32();
        Header.FilenamesOffset = R.ReadUInt32();
    }

    public static PakVersion GetPakVersion(string Magic)
    {
        switch(Magic)
        {
            case "P4CK":
                return PakVersion.P4CK;
            case "P5CK":
                return PakVersion.P5CK;
            case "P8CK":
                return PakVersion.P8CK;

            default:
                return PakVersion.UNKNOWN;
        }
    }

    private TSPakEntry ReadDirEntryV4(BinaryReader R)
    {
        const int FILENAME_LENGTH = 48;
        var entry = new TSPakEntry()
        {
            Name   = new string(R.ReadChars(FILENAME_LENGTH)).TrimEnd(new char[] { ' ', (char)0 }),
            Offset = R.ReadUInt32(),
            Size   = R.ReadUInt32()
        };

        // Unknown
        R.ReadUInt32();

        return entry;
    }

    private void LoadDirEntriesV4(BinaryReader R)
    {
        const int ENTRY_SIZE = 60;
        int numEntries       = (int)(Header.DirSize / ENTRY_SIZE);

        R.BaseStream.Seek(Header.DirOffset, SeekOrigin.Begin);

        for (int i = 0; i < numEntries; i++)
        {
            var entry = ReadDirEntryV4(R);
            if (!FileEntries.ContainsKey(entry.Name))
            {
                FileEntries.Add(entry.Name, entry);
            }
        }
    }

    private TSPakEntry ReadDirEntryV5(BinaryReader R)
    {
        var entry = new TSPakEntry()
        {
            Name   = $"{R.ReadUInt32()}",
            Offset = R.ReadUInt32(),
            Size   = R.ReadUInt32(),
            Extra  = R.ReadUInt32()
        };

        return entry;
    }

    private void LoadDirEntriesV5(BinaryReader R)
    {
        const int ENTRY_SIZE = 32;
        int numEntries       = (int)(Header.DirSize / ENTRY_SIZE);
        var entries          = new TSPakEntry[numEntries];
        var fileNames        = GetFilenameLookups(PakFilePath);

        R.BaseStream.Seek(Header.DirOffset, SeekOrigin.Begin);

        for (int i = 0; i < numEntries; i++)
        {
            var entry          = ReadDirEntryV5(R);
            var idAsUint       = uint.Parse(entry.Name);
            var lookupFilename = fileNames != null ? fileNames[idAsUint] : null;
            var name           = lookupFilename ?? $"{entry.Name}_{entry.Offset}_{entry.Size}_{entry.Extra}";

            if (!FileEntries.ContainsKey(name))
            {
                FileEntries.Add(name, entry);
            }
        }
    }

    private TSPakEntry ReadDirEntryV8(BinaryReader R)
    {
        var entry = new TSPakEntry()
        {
            Extra  = R.ReadUInt32(),
            Size   = R.ReadUInt32(),
            Offset = R.ReadUInt32()
        };

        return entry;
    }

    private string ReadNullTermString(BinaryReader R)
    {
        var sb = new StringBuilder();

        while (R.PeekChar() != 0)
        {
            sb.Append(R.ReadChar());
        }

        // Take the null char
        R.ReadChar();

        return sb.ToString();
    }

    private string[] LoadFilenamesV8(BinaryReader R)
    {
        var fileNames = new string[Header.DirSize];

        R.BaseStream.Seek(Header.FilenamesOffset, SeekOrigin.Begin);
        for (int i = 0; i < fileNames.Length; i++)
        {
            var fileName = ReadNullTermString(R);
            fileNames[i] = fileName;
        }

        return fileNames;
    }

    private void LoadDirEntriesV8(BinaryReader R)
    {
        var entries          = new TSPakEntry[Header.DirSize];
        var fileNames        = LoadFilenamesV8(R);

        R.BaseStream.Seek(Header.DirOffset, SeekOrigin.Begin);

        for (int i = 0; i < Header.DirSize; i++)
        {
            var entry  = ReadDirEntryV8(R);
            entry.Name = fileNames[i];

            if (!FileEntries.ContainsKey(entry.Name))
            {
                FileEntries.Add(entry.Name, entry);
            }
            else
            {
                FileEntries.Add($"{entry.Name}_{i}", entry);
            }
        }
    }

    // Looks for a name lookup file near the pak, and if found retuns a look up
    private FileNameLookups GetFilenameLookups(string PakFilePath)
    {
        string[] FILE_EXTS = new string[] { "c2n" };
        var lookups        = new FileNameLookups();
        var baseFileName   = Path.GetFileNameWithoutExtension(PakFilePath);
        var filesToTry     = FILE_EXTS.Select(x => Path.Combine(Path.GetDirectoryName(PakFilePath), $"{baseFileName}.{x}")).ToList();

        foreach (var filePath in filesToTry)
        {
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var tokens = line.Split(new string[] { "  " }, StringSplitOptions.None); // double space
                    uint id    = uint.Parse(tokens[0].Substring(2), System.Globalization.NumberStyles.HexNumber);
                    var name   = tokens[1];

                    if (!lookups.ContainsKey(id))
                    {
                        lookups.Add(id, name);
                    }
                }

                return lookups;
            }
        }

        return null;
    }
}

public struct TSPakHeader
{
    public char[] Magic;
    public uint DirOffset;
    public uint DirSize;
    public uint FilenamesOffset; // for p8ck and some others
}

// Common file entry
public struct TSPakEntry
{
    public string Name; // or id as a string if format has none
    public uint Offset;
    public uint Size;
    public uint Extra;
}

public enum PakVersion
{
    P4CK,
    P5CK,
    P8CK,
    UNKNOWN
}
