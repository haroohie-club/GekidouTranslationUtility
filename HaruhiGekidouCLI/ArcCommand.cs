using System.Runtime.InteropServices.JavaScript;
using System.Text;
using HaruhiGekidouLib;
using HaruhiGekidouLib.Archive;
using HaruhiGekidouLib.Util;
using Mono.Options;

namespace HaruhiGekidouCLI;

public class ArcCommand : Command
{
    private string _input = string.Empty, _output = string.Empty;
    private bool _extract, _pack, _dumpCsv;
    
    public ArcCommand() : base("arc", "Various functions to deal with script archives")
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

        if (!Directory.Exists(Path.GetDirectoryName(_output)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_output)!);
        }

        if (_extract)
        {
            byte[] arcBytes = File.ReadAllBytes(_input);
            if (_input.EndsWith(".lz77"))
            {
                arcBytes = Compression.Decompress(arcBytes);
            }
            
            GekidouArc arc = new(arcBytes);
            Directory.CreateDirectory(_output);
            string currentDir = _output;
            int currentDepth = 0;
            foreach (GekidouArcEntry entry in arc.Entries)
            {
                if (entry.IsDirectory)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }
                    
                    while (currentDepth > 0 && currentDepth >= entry.OffsetOrDepth)
                    {
                        currentDir = Path.GetDirectoryName(currentDir) ?? string.Empty;
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
            GekidouArc arc = new();
            string[] directories = 
            [
                _input,
                .. Directory.GetDirectories(_input, "*", SearchOption.AllDirectories).Order()
            ];
            
            arc.Entries.Add(new(string.Empty, true, depth: 0));
            foreach (string dir in directories)
            {
                int actualDepth = dir.Equals(_input) ? 1 : Path.GetRelativePath(_input, dir).Split(Path.DirectorySeparatorChar).Length + 1;
                
                int depth = 0xFF >> (9 - actualDepth);
                int lastItemIdx = 1 + actualDepth + Directory.GetFileSystemEntries(dir, "*", SearchOption.AllDirectories).Length
                                  + (arc.Entries.LastOrDefault(e => e.OffsetOrDepth == depth)?.LengthOrLastItemIdx ?? 0);   //potential issue here: tutorial_000 is incorrect at AdvPartScript
                arc.Entries.Add(new(Path.GetFileName(dir), true, depth, lastItemIdx));
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    arc.Entries.Add(new(Path.GetFileName(file), false, data: File.ReadAllBytes(file)));
                }
            }

            byte[] arcBytes = arc.GetBytes();
            if (_output.EndsWith(".lz77"))
            {
                arcBytes = Compression.Compress(arcBytes);
            }
            File.WriteAllBytes(_output, arcBytes);
        }
        else if (_dumpCsv)
        {
            GekidouArc arc = new(File.ReadAllBytes(_input));
            StringBuilder sb = new();
            sb.AppendLine($"{nameof(GekidouArcEntry.Name)},{nameof(GekidouArcEntry.IsDirectory)},{nameof(GekidouArcEntry.OffsetOrDepth)},{nameof(GekidouArcEntry.LengthOrLastItemIdx)}");
            foreach (GekidouArcEntry entry in arc.Entries)
            {
                sb.AppendLine($"{entry.Name},{entry.IsDirectory},{entry.OffsetOrDepth},{entry.LengthOrLastItemIdx}");
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