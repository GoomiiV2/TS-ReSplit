using CommandLine;
using System;
using System.Drawing;
using System.IO;

namespace ReSplit_Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmdOpts = CommandLine.Parser.Default.ParseArguments<CmdOpts>(args).WithParsed<CmdOpts>(o =>
            {
                if (o.Listing)
                {
                    PrintListing(o);
                }

                if (o.extract)
                {
                    if (o.Output != null)
                    {
                        Extract(o);
                    }
                    else
                    {
                        Console.WriteLine("No output path given, please give a path to extract the files to with -o");
                    }
                }
            });
        }

        private static TSPak LoadPak(string PakPath)
        {
            var pak = new TSPak();
            pak.LoadEntries(PakPath);

            return pak;
        }

        private static void PrintListing(CmdOpts Opts)
        {
            var filesPaths = Opts.Input.Split(',');

            foreach (var filePath in filesPaths)
            {
                var pak       = LoadPak(filePath);
                var fileNames = pak.GetFileList();
                Console.WriteLine($"Pak File: {filePath}");
                Console.WriteLine($"Version: {new string(pak.Header.Magic)}"
);
                Console.WriteLine($"File Entires: {fileNames.Count}");
                Console.WriteLine("");

                Console.WriteLine("Files: ");
                foreach (var fileName in fileNames)
                {
                    Console.WriteLine(fileName);
                }
            }
        }

        private static void Extract(CmdOpts Opts)
        {
            var filesPaths = Opts.Input.Split(',');

            foreach (var filePath in filesPaths)
            {
                Console.WriteLine($"Extracting files from: {filePath}");

                var pak       = LoadPak(filePath);
                var fileNames = pak.GetFileList();

                foreach (var fileName in fileNames)
                {
                    var pakName = Path.GetFileName(filePath);
                    var outPath = Path.Combine(Opts.Output, pakName, fileName);
                    var data    = pak.GetFile(fileName);
                    ExtractFile(data, outPath);
                }

                Console.WriteLine("Done");
            }
        }

        private static void ExtractFile(byte[] Data, string FilePath)
        {
            var dirPath  = Path.GetDirectoryName(FilePath);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            File.WriteAllBytes(FilePath, Data);
        }
    }
}
