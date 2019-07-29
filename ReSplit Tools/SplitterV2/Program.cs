using CommandLine;
using Newtonsoft.Json;
using ReSplit_Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace SplitterV2
{
    class Program
    {
        const string FILE_LISTING_NAME                       = "FileListing.json";
        static readonly string[] SUPPORTED_REPACKING_FORMATS = new string[] { "P4CK" };

        static void Main(string[] args)
        {
            Console.WriteLine("Splitter, A TimeSpliters / Second Sight Pak extractor and packer", Color.Pink);
            Console.WriteLine(GetLinkerTime(Assembly.GetExecutingAssembly()).ToLongDateString(), Color.Pink);
            Console.WriteLine("Arkii :>", Color.Pink);
            Console.WriteLine();

            var cmdOpts = CommandLine.Parser.Default.ParseArguments<CmdArgs>(args).WithParsed<CmdArgs>(o =>
            {
                if (o.Extract)
                {
                    if (o.Input  == null) { Console.WriteLine("You need to specify a pak file to extract"); }
                    if (o.Output == null) { Console.WriteLine("You need to specify where to extract the files to"); }

                    if (o.Input != null && o.Output != null)
                    {
                        var paths = o.Input.Split(new char[] { ',' });
                        ExtractPaks(paths, o.Output);
                    }
                }
                else if (o.Pack)
                {
                    if (o.Input  == null) { Console.WriteLine($"You need to specify a {FILE_LISTING_NAME} file, listing all the files and pak version to pak into"); }
                    if (o.Output == null) { Console.WriteLine("You need to specify where put the built pak"); }

                    if (o.Input != null && o.Output != null)
                    {
                        var path = !o.Input.EndsWith(FILE_LISTING_NAME) ? Path.Combine(o.Input, FILE_LISTING_NAME) : o.Input;
                        PakFiles(path, o.Output);
                    }
                }
            });
        }

        static void ExtractPaks(string[] Paks, string OutPath)
        {
            Console.WriteLine($"Extracting {Paks.Length} paks to {OutPath}...", Color.Teal);
            foreach (var pakPath in Paks)
            {
                if (File.Exists(pakPath))
                {
                    var outDir          = Path.Combine(OutPath, Path.GetFileNameWithoutExtension(pakPath));
                    var fileListingPath = Path.Combine(outDir, FILE_LISTING_NAME);
                    Directory.CreateDirectory(outDir);

                    var pak = new TSPak(pakPath);
                    Console.WriteLine($"Extracting {pakPath} ({new string(pak.Header.Magic)}) to {outDir}", Color.Coral);
                    WritePakFileListing(pak, pakPath, fileListingPath);
                    Console.WriteLine($"Wrote file listing", Color.Coral);
                    ExtractPak(pak, outDir);
                    Console.WriteLine($"All files extracted :>", Color.Coral);
                }
                else
                {
                    Console.WriteLine($"Couldn't find pak file at {pakPath}, skipping :<", Color.Red);
                }
            }
            Console.WriteLine($"Done :>", Color.Teal);
            Console.WriteLine();
        }

        static void ExtractPak(TSPak Pak, string OutDir)
        {
            var files = Pak.GetFileList();
            Console.WriteLine($"Extracting {files.Count} files to {OutDir}", Color.Coral);

            foreach (var fileName in files)
            {
                var outPath = Path.Combine(OutDir, fileName);
                var file    = Pak.GetFile(fileName);

                var outDirPath = Path.GetDirectoryName(outPath);
                if (!Directory.Exists(outDirPath))
                {
                    Directory.CreateDirectory(outDirPath);
                }

                File.WriteAllBytes(outPath, file);
            }
        }

        // The file listing is used to preserve the file order for repacking as TimeSplitters 2 atleast gets cranky if some files are out of order
        static void WritePakFileListing(TSPak Pak, string PakPath, string OutFilePath)
        {
            var files = Pak.GetFileList();
            var info = new PakFileListing()
            {
                PakFileInfo = new PakFileListing.PakInfo()
                {
                    PakMagic = new string(Pak.Header.Magic),
                    PakName  = Path.GetFileName(PakPath)
                },

                Files = files.ToArray()
            };

            var jsonText = JsonConvert.SerializeObject(info, Formatting.Indented);
            File.WriteAllText(OutFilePath, jsonText);
        }

        static void PakFiles(string FileListingPath, string PakPath)
        {
            var fileListingText = File.ReadAllText(FileListingPath);
            var fileListing     = JsonConvert.DeserializeObject<PakFileListing>(fileListingText);

            // Check if its a supported format for repacking
            if (!SUPPORTED_REPACKING_FORMATS.Contains(fileListing.PakFileInfo.PakMagic))
            {
                Console.WriteLine($"Sorry packing of {fileListing.PakFileInfo.PakMagic} paks isn't supported yet :<");
                return;
            }

            var baseDir = Path.GetDirectoryName(FileListingPath);
            var packer  = new Packer(PakPath, TSPak.GetPakVersion(fileListing.PakFileInfo.PakMagic));

            Console.WriteLine($"Adding {fileListing.Files.Length} files from {baseDir} to {PakPath} of type {fileListing.PakFileInfo.PakMagic}....", Color.Teal);

            foreach (var filePath in fileListing.Files)
            {
                var path = Path.Combine(baseDir, filePath);
                packer.AddFile(path, filePath);
            }

            packer.Finsih();
            Console.WriteLine($"Pak writen :>", Color.Teal);
        }

        #region Utils
        // https://stackoverflow.com/a/1600990
        public static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }
        #endregion
    }
}
