using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spindle
{
    public class CmdArgs
    {
        [Option('i', "input", Required = true, HelpText = "Path to a folder or an ISO image to be packed ot extracted from")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Path to a folder or an iso to be extracted or packed to")]
        public string Output { get; set; }

        [Option('e', "extract", Required = false, HelpText = "Extract the files from an ISO")]
        public bool Extract { get; set; }

        [Option('p', "pack", Required = false, HelpText = "Pack the files to an ISO")]
        public bool Pack { get; set; }
    }
}
