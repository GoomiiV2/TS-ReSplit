using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TSPak
{
    private TSPakHeader Header;
    private Dictionary<string, TSPakEntry> FileEntries = new Dictionary<string, TSPakEntry>();
    private string PakFilePath;

    public void LoadEntries(string FilePath)
    {
        PakFilePath = FilePath;

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

                    }
                    else if (headerMagic == "P4CK") // Timesplitters 2
                    {
                        LoadDirEntriesV4(reader);
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
            using (var file = File.Open(PakFilePath, FileMode.Open))
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
        Header.Magic     = R.ReadChars(4);
        Header.DirOffset = R.ReadUInt32();
        Header.DirSize   = R.ReadUInt32();
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
            FileEntries.Add(entry.Name, entry);
        }
    }

    private TSPakEntry ReadDirEntryV5(BinaryReader R)
    {
        var entry = new TSPakEntry()
        {
            Name   = $"{R.ReadUInt32()}",
            Offset = R.ReadUInt32(),
            Size   = R.ReadUInt32()
        };

        // Unknown
        R.ReadUInt32();

        return entry;
    }

    private TSPakEntry[] ReadDirEntriesV5(BinaryReader R)
    {
        const int ENTRY_SIZE = 32;
        int numEntries       = (int)(Header.DirSize / ENTRY_SIZE);
        var entries          = new TSPakEntry[numEntries];

        R.BaseStream.Seek(Header.DirOffset, SeekOrigin.Begin);

        for (int i = 0; i < numEntries; i++)
        {
            entries[i] = ReadDirEntryV5(R);
        }

        return entries;
    }
}

public struct TSPakHeader
{
    public char[] Magic;
    public uint DirOffset;
    public uint DirSize;
}

// Common file entry
public struct TSPakEntry
{
    public string Name; // or id as a string if format has none
    public uint Offset;
    public uint Size;
}
