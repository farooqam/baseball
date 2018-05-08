using CommandLine;

namespace Retrosheet.Utilities.GameLogUtility.CommandLine
{
    [Verb("processgamelog", HelpText = "Process game log files.")]
    public class ProcessGameLogOptions
    {
        [Option('i', "input", Required = true, HelpText = "The directory containing game log files.")]
        public string InputDirectory { get; set; }
    }
}