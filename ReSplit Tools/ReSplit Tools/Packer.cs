using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReSplit_Tools
{
    public class Packer
    {
        const int PakV4PaddingAmount = 2048;

        private PakVersion Version;
        private uint DirOffset         = 0;
        private uint DirLength         = 0;
        private uint FileListingOffset = 0;
        public List<FileData> FileInfo = new List<FileData>();
        private BinaryWriter Writer    = null;

        public Packer(string Filepath, PakVersion Ver)
        {
            Version = Ver;

            if (File.Exists(Filepath))
            {
                File.Delete(Filepath);
            }

            Writer = new BinaryWriter(File.OpenWrite(Filepath));
            WriteHeader();

            if (Version == PakVersion.P4CK)
            {
                Writer.Write("".PadRight(2032, (char)0).ToCharArray()); // Padding
            }
        }

        public void AddFolder(string DirPath)
        {
            var files = Directory.GetFiles(DirPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                var fileName = filePath.Replace(DirPath, "").Substring(1);
                AddFile(filePath, fileName);
            }
        }

        public void AddFile(string Filepath, string FileName)
        {
            var data = File.ReadAllBytes(Filepath);

            switch (Version)
            {
                case PakVersion.P4CK:
                    AddFileV4(data, FileName);
                    break;
            }
        }


        private void AddFileV4(byte[] Data, string FileName)
        {
            var fileData = new FileData()
            {
                Name   = FileName,
                Offset = (uint)Writer.BaseStream.Position,
                Length = (uint)Data.Length
            };


            FileInfo.Add(fileData);

            Writer.Write(Data);

            // Work out how much padding is needed
            var powerOfTwoMultiple = Math.Ceiling((decimal)Data.Length / (decimal)PakV4PaddingAmount);
            var paddingAmount      = (int)((PakV4PaddingAmount * powerOfTwoMultiple) - Data.Length);
            Writer.Write("".PadRight(paddingAmount, (char)0).ToCharArray());
        }

        public void Finsih()
        {
            switch (Version)
            {
                case PakVersion.P4CK:
                    FinishV4();
                    break;
            }

            WriteHeader();
            Writer.Flush();
            Writer.Close();
        }

        private void FinishV4()
        {
            DirOffset = (uint)Writer.BaseStream.Position;
            DirLength = (uint)(FileInfo.Count * 60);

            foreach (var fileEntry in FileInfo)
            {
                var name = fileEntry.Name.PadRight(48, (char)0);
                Writer.Write(name.ToCharArray());
                Writer.Write(fileEntry.Offset);
                Writer.Write(fileEntry.Length);
                Writer.Write(fileEntry.Extra);
            }
        }

        public void WriteHeader()
        {
            Writer.BaseStream.Seek(0, SeekOrigin.Begin);
            var magic = GetPakVersionMagic(Version);
            Writer.Write(magic);
            Writer.Write(DirOffset);
            Writer.Write(DirLength);
            Writer.Write(FileListingOffset);
        }

        private char[] GetPakVersionMagic(PakVersion Ver)
        {
            switch (Ver)
            {
                case PakVersion.P4CK:
                    return "P4CK".ToCharArray();

                case PakVersion.P5CK:
                    return "P5CK".ToCharArray();

                case PakVersion.P8CK:
                    return "P8CK".ToCharArray();

                default:
                    return "NOPE".ToCharArray();
            }
        }
    }

    public struct FileData
    {
        public string Name;
        public uint? ID;
        public uint Offset;
        public uint Length;
        public uint Extra;
    }
}
