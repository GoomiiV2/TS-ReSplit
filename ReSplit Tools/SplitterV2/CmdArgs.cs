using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitterV2
{
    public class CmdArgs
    {
        [Option('i', "input", Required = true, HelpText = "Filenames or filepaths to the paks, if multiple seprate with a ,")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Folder to place the unpacked files at")]
        public string Output { get; set; }

        [Option('e', "extract", Required = false, HelpText = "Extract the files from the paks")]
        public bool Extract { get; set; }

        [Option('p', "pack", Required = false, HelpText = "Path to a FileListing.json file to repack")]
        public bool Pack { get; set; }
    }
}
