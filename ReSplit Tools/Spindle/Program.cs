using CommandLine;
using DiscUtils.Iso9660;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Spindle
{
    class Program
    {
        const string IsoInfoName = "ISOInfo.json";

        // A simple tool to pack and unpack an iso
        static void Main(string[] args)
        {
            Console.WriteLine("Spindle, A simple ISO packer / extractor", Color.Pink);
            Console.WriteLine("This is pretty much just a wrapper around DiscUtils", Color.Pink);

            var cmdOpts = CommandLine.Parser.Default.ParseArguments<CmdArgs>(args).WithParsed<CmdArgs>(o =>
            {
                if (o.Extract)
                {
                    if (o.Input == null) { Console.WriteLine("You need to specify an ISO to extract from"); }
                    else
                    {
                        var outPath = o.Output == null
                                ? Path.Combine(Path.GetDirectoryName(o.Input), Path.GetFileNameWithoutExtension(o.Input))
                                : o.Output;

                        Extract(o.Input, outPath);
                    }
                }
                else if(o.Pack)
                {
                    if (o.Input == null) { Console.WriteLine("You need to specify a folder to pack"); }
                    else
                    {
                        var outPath = o.Output == null
                                ? Path.Combine(Path.GetDirectoryName(o.Input), $"{new DirectoryInfo(o.Input).Name}.iso")
                                : o.Output;

                        Pack(o.Input, outPath);
                    }
                }
            });
        }

        static async void Extract(string ISOPath, string OutDir)
        {
            if (!File.Exists(ISOPath)) { Console.WriteLine($"Couldn't find file at {ISOPath} :<", Color.Red); return; }
            Directory.CreateDirectory(OutDir);
            var sw = Stopwatch.StartNew();

            Console.WriteLine($"Extracting files from {ISOPath} to {OutDir}", Color.Teal);
            using (FileStream isoStream = File.OpenRead(ISOPath))
            {
                CDReader cd        = new CDReader(isoStream, true);
                var writeTasks     = new List<Task>();

                var isoInfo = new ISOData()
                {
                    VolumeLabel = cd.VolumeLabel
                };
                var isoInfoJson = JsonConvert.SerializeObject(isoInfo);
                File.WriteAllText(Path.Combine(OutDir, IsoInfoName), isoInfoJson);

                async void findFilesInDir(DiscUtils.DiscDirectoryInfo Dir)
                {
                    var toWriteFiles = new Queue<(string FileName, byte[] Data)>();

                    foreach (var fileInfo in Dir.GetFiles())
                    {
                        using (var fileStream = fileInfo.OpenRead())
                        {
                            byte[] fileData = new byte[fileInfo.Length];
                            fileStream.Read(fileData, 0, fileData.Length);
                            toWriteFiles.Enqueue((fileInfo.FullName, fileData));
                        }
                    }

                    var writeTask = Task.Factory.StartNew(() =>
                    {
                        while(toWriteFiles.Count > 0)
                        {
                            var file     = toWriteFiles.Dequeue();
                            var fileName = file.FileName.Substring(0, file.FileName.Length - 2);
                            var outPath  = Path.Combine(OutDir, fileName);
                            if (!Directory.Exists(outPath)) { Directory.CreateDirectory(Path.GetDirectoryName(outPath)); }

                            Console.WriteLine($"Writing {fileName}");
                            File.WriteAllBytes(outPath, file.Data);
                        }

                        GC.Collect();
                    });
                    writeTasks.Add(writeTask);

                    foreach (var dir in Dir.GetDirectories())
                    {
                        findFilesInDir(dir);
                    }
                }

                findFilesInDir(cd.Root);

                Task.WaitAll(writeTasks.ToArray());
                sw.Stop();

                Console.WriteLine($"Extracted in {sw.Elapsed}");
            }
        }

        static void Pack(string InDir, string OutISOPath)
        {
            var sw                   = Stopwatch.StartNew();
            CDBuilder builder        = new CDBuilder();
            builder.UseJoliet        = true;
            var builderLockObj       = new object();

            // Try and load meta info saved from an extraction
            var metaFilePath = Path.Combine(InDir, IsoInfoName);
            if (File.Exists(metaFilePath))
            {
                var jsonText = File.ReadAllText(metaFilePath);
                var isoInfo  = JsonConvert.DeserializeObject< ISOData>(jsonText);

                builder.VolumeIdentifier = isoInfo.VolumeLabel;
            }

            var files = Directory.GetFiles(InDir, "*", SearchOption.AllDirectories).Distinct().ToList();
            Console.WriteLine($"Packing files from {files.Count()} to {OutISOPath}", Color.Teal);
            //Parallel.ForEach(files, (file) =>
            files.ForEach((file) =>
            {
                if (file.ToUpper() != IsoInfoName.ToUpper())
                {
                    var data     = File.ReadAllBytes(file);
                    var fileName = file.Replace(InDir, "");

                    Console.WriteLine($"Adding file: {fileName}");
                    lock (builderLockObj)
                    {
                        builder.AddFile($"{fileName};1", data);
                    }
                }
            });

            builder.Build(OutISOPath);
            sw.Stop();



            //cdrom0:¥SLES_508.77;1
        }
    }
}
