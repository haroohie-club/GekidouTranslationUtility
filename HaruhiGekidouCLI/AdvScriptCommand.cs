using System.Text.Json;
using HaruhiGekidouLib.Archive;
using HaruhiGekidouLib.Script;
using Mono.Options;

namespace HaruhiGekidouCLI;

public class AdvScriptCommand : Command
{
    private bool _dump, _extract, _replace;
    private string _scriptPath, _outputPath, _jsonPath;
    
    public AdvScriptCommand() : base("adv-script")
    {
        Options = new()
        {
            { "d|dump", "Dumps the script to a CSV file", _ => _dump = true },
            { "x|extract", "Extracts the script's dialogue to a JSON file", _ => _extract = true },
            { "r|replace", "Replaces the script's dialogue with lines from a JSON file", _ => _replace = true },
            { "i|input=", "Input script file", i => _scriptPath = i },
            { "j|json=", "Path to replacement JSON file, used with -r", j => _jsonPath = j },
            { "o|output=", "Output file", o => _outputPath = o },
        };
    }

    public override int Invoke(IEnumerable<string> arguments)
    {
        Options.Parse(arguments);

        if (!Directory.Exists(Path.GetDirectoryName(_outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_outputPath)!);
        }

        if (_dump)
        {
            AdvPartScript script = new(Path.GetFileNameWithoutExtension(_scriptPath),
                File.ReadAllBytes(_scriptPath));
            File.WriteAllText(_outputPath, script.ExportCsv());
        }
        else if (_extract)
        {
            AdvPartScript script = new(Path.GetFileNameWithoutExtension(_scriptPath),
                File.ReadAllBytes(_scriptPath));
            File.WriteAllText(_outputPath, script.ExportDialogueJson());
        }
        else if (_replace)
        {
            AdvPartScript script = new(Path.GetFileNameWithoutExtension(_scriptPath),
                File.ReadAllBytes(_scriptPath));
            script.ImportDialogueJson(File.ReadAllText(_jsonPath));
            File.WriteAllBytes(_outputPath, script.GetBytes());
        }
        else
        {
            Options.WriteOptionDescriptions(CommandSet.Out);
        }
        
        return 0;
    }
}