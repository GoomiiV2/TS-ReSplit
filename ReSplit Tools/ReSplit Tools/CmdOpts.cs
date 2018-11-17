using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReSplit_Tools
{
    public class CmdOpts
    {
        [Option('i', "input", Required = true, HelpText = "Filenames or filepaths to the paks, if multiple seprate with a ,")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Folder to place the unpacked files at")]
        public string Output { get; set; }

        [Option('m', "manifest", Required = false, HelpText = "Export a JSON file with the file listing and info to the given path")]
        public string Manifest { get; set; }

        [Option('e', "extract", Required = false, HelpText = "Extract the files")]
        public bool extract { get; set; }

        [Option('l', "listing", Required = false, HelpText = "Print info about the give pak file and a list")]
        public bool Listing { get; set; }

        [Option('p', "pack", Required = false, HelpText = "Pack the file in the given folder in to a pak witht eh given name in output, version is either P4CK, P5CK, P8CK")]
        public string Pack { get; set; }
    }
}
