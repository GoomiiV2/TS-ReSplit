using DotNetGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ts2MeshGrapher
{
    class Program
    {
        const string PakPath = @"F:Projects2\Code\Timesplitters FP framework\Unity Project\Data\ts2\pak/chrinc.pak";
        const string OutDir = @"F:\Projects2\Code\Timesplitters FP framework\Temp\Mesh Graphs";
        private static string PakName;

        static void Main(string[] args)
        {
            PakName = Path.GetFileName(PakPath);

            var pak = new TSPak();
            pak.LoadEntries(PakPath);

            foreach (var entry in pak.GetFileList())
            {
                if (entry.StartsWith("ob"))
                {
                    var fileData = pak.GetFile(entry);
                    var model    = new TS2.Model(fileData);
                    //CreateMeshLinkGraph(model, entry);
                    PrintMeshInfos(model, entry);
                }
            }
        }

        private static void CreateMeshLinkGraph(TS2.Model Model, string FileName)
        {
            var graph = new DotGraph($"Mesh Link Graph: {FileName}");
            var nodes = new List<DotNode>(Model.MeshInfos.Length);

            // Create the nodes
            for (int i = 0; i < Model.MeshInfos.Length; i++)
            {
                var meshInfo = Model.MeshInfos[i];
                var node = new DotNode($"Mesh {i}")
                {
                    Label = $"Mesh {i}"
                };

                graph.Add(node);
                nodes.Add(node);
            }

            // Link the nodes
            for (int i = 0; i < Model.MeshInfos.Length; i++)
            {
                var meshInfo = Model.MeshInfos[i];
                if (meshInfo.HasChild)
                {
                    var arrow = new DotArrow(nodes[i], nodes[meshInfo.ChildIdx]);
                    graph.Add(arrow);
                }
            }

            var graphFile   = graph.Compile(false);
            var outFileName = Path.Combine(OutDir, PakName, $"{FileName}.dot");
            var outDir      = Path.GetDirectoryName(outFileName);
            Directory.CreateDirectory(outDir);
            File.WriteAllText(outFileName, graphFile);
        }

        private static void PrintMeshInfos(TS2.Model Model, string FileName)
        {
            var sb = new StringBuilder();
            var boneCount = 0;

            for (int i = 0; i < Model.MeshInfos.Length; i++)
            {
                var meshInfo = Model.MeshInfos[i];
                var meshInfoStr = $"{i,2}  {meshInfo.IsBone,3}  {meshInfo.ParentIdx,3}  {meshInfo.ChildIdx,3}  {meshInfo.Unk2,3}  {meshInfo.Unk4,3}  {meshInfo.HasChild,5} ";

                sb.AppendLine(meshInfoStr);

                if (meshInfo.IsBone > 0)
                {
                    boneCount++;
                }
            }

            sb.AppendLine();
            sb.AppendLine($"Bones: {boneCount}");

            var outFileName = Path.Combine(OutDir, PakName, $"{FileName}.txt");
            var outDir      = Path.GetDirectoryName(outFileName);
            Directory.CreateDirectory(outDir);
            File.WriteAllText(outFileName, sb.ToString());
        }
    }
}
