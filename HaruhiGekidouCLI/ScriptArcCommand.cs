using System.Text;
using HaruhiGekidouLib;
using Mono.Options;

namespace HaruhiGekidouCLI;

public class ScriptArcCommand : Command
{
    private string _input, _output;
    private bool _extract, _pack, _dumpCsv;
    
    public ScriptArcCommand() : base("script-arc", "Various functions to deal with script archives")
    {
        Options = new()
        {
            { "x|extract", "Extracts all the contents of the archive to a directory", _ => _extract = true },
            { "p|pack", "Packs all the contents of a directory into a script archive", _ => _pack = true },
            { "d|dump|dump-csv", "Dumps data on the archive to a CSV file", _ => _dumpCsv = true },
            { "i|input=", "The path to the input script archive or directory", i => _input = i },
            { "o|output=", "The path to the output directory, script archive, or CSV", o => _output = o },
        };
    }

    public override int Invoke(IEnumerable<string> arguments)
    {
        Options.Parse(arguments);

        if (_extract)
        {
            ScriptArc arc = new(File.ReadAllBytes(_input));
            Directory.CreateDirectory(_output);
            string currentDir = _output;
            int currentDepth = 0;
            foreach (ScriptArcEntry entry in arc.Entries)
            {
                if (entry.IsDirectory)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }
                    
                    while (currentDepth > 0 && currentDepth >= entry.OffsetOrDepth)
                    {
                        currentDir = Path.GetDirectoryName(currentDir);
                        currentDepth >>= 1;
                    }
                    currentDir = Path.Combine(currentDir, entry.Name);
                    currentDepth = entry.OffsetOrDepth;
                    Directory.CreateDirectory(currentDir);
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(currentDir, entry.Name), entry.Data);
                }
            }
        }
        else if (_pack)
        {
            
        }
        else if (_dumpCsv)
        {
            ScriptArc arc = new(File.ReadAllBytes(_input));
            StringBuilder sb = new();
            sb.AppendLine($"{nameof(ScriptArcEntry.Name)},{nameof(ScriptArcEntry.IsDirectory)},{nameof(ScriptArcEntry.NameOffset)},{nameof(ScriptArcEntry.OffsetOrDepth)},{nameof(ScriptArcEntry.LengthOrLastItemIdx)}");
            foreach (ScriptArcEntry entry in arc.Entries)
            {
                sb.AppendLine($"{entry.Name},{entry.IsDirectory},{entry.NameOffset},{entry.OffsetOrDepth},{entry.LengthOrLastItemIdx}");
            }
            File.WriteAllText(_output, sb.ToString());
        }
        else
        {
            Options.WriteOptionDescriptions(CommandSet.Out);
        }
        
        return 0;
    }
}