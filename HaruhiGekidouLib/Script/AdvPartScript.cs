using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using HaruhiGekidouLib.Util;

namespace HaruhiGekidouLib.Script;

public partial class AdvPartScript
{
    public string Name { get; set; }
    
    public int Version { get; set; }
    
    public byte[] PreScriptData { get; set; }

    public List<AdvPartScriptBlock> ScriptBlocks { get; set; } = [];
    
    private static JsonSerializerOptions _options = new(new JsonSerializerOptions()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    public AdvPartScript(string name, byte[] data)
    {
        Name = name;
        Version = IO.ReadInt(data, 0x00);
        if (Version != 8)
        {
            throw new InvalidDataException($"Incorrect version! Expected 8, actual {Version}");
        }
        
        int scriptBlockOffsetsOffset = IO.ReadInt(data, 0x04);
        PreScriptData = data[..scriptBlockOffsetsOffset];
        
        int numBlocks = IO.ReadInt(data, scriptBlockOffsetsOffset);
        
        int[] scriptBlockOffsets = new int[numBlocks];
        for (int i = 1; i <= numBlocks; i++)
        {
            scriptBlockOffsets[i - 1] = IO.ReadInt(data, scriptBlockOffsetsOffset + i * 0x04);
        }

        foreach (int scriptBlockOffset in scriptBlockOffsets)
        {
            int blockLength = IO.ReadInt(data, scriptBlockOffsetsOffset + scriptBlockOffset);
            ScriptBlocks.Add(new(
                data[(scriptBlockOffsetsOffset + scriptBlockOffset + 4)..(scriptBlockOffsetsOffset + scriptBlockOffset + 4 + blockLength)]));
        }
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = [.. PreScriptData];

        List<byte[]> scriptBlockBytes = [.. ScriptBlocks.Select(b => b.GetBytes())];
        
        bytes.AddRange(IO.GetIntBytes(ScriptBlocks.Count));
        int startLocation = 0x104;
        foreach (byte[] data in scriptBlockBytes)
        {
            bytes.AddRange(IO.GetIntBytes(startLocation));
            startLocation += data.Length + (0x20 - (startLocation+data.Length) % 0x20);
                             
                             //((data.Length+0x104) % 0x20);
        }
        for (int i = 0; i < 64 - ScriptBlocks.Count; i++)
        {
            bytes.AddRange(IO.GetIntBytes(-1));
        }

        foreach (byte[] data in scriptBlockBytes)
        {
            bytes.AddRange(data);
            if (bytes.Count % 0x20 != 0)
            {
                bytes.AddRange(new byte[0x20 - bytes.Count() % 0x20]);
            }
        }
        

        
        return [.. bytes];
    }

    public string ExportDialogueJson()
    {
        Dictionary<string, string> lines = [];

        for (int i = 0; i < ScriptBlocks.Count; i++)
        {
            Speaker mostRecentSpeaker = Speaker.Monologue;
            bool moreRecent14 = false;
            for (int j = 0; j < ScriptBlocks[i].Commands.Count; j++)
            {
                if (ScriptBlocks[i].Commands[j].Command == 13 || ScriptBlocks[i].Commands[j].Command == 30)
                {
                    moreRecent14 = false;
                    int speakerInt = ScriptBlocks[i].Commands[j].Parameters[ScriptBlocks[i].Commands[j].Command == 13 ? 3 : 1];
                    speakerInt = speakerInt > 20 ? speakerInt - 20 : speakerInt;
                    mostRecentSpeaker = (Speaker)speakerInt;
                }
                else if (ScriptBlocks[i].Commands[j].Command == 14)
                {
                    moreRecent14 = true;
                }
                else if (ScriptBlocks[i].Commands[j].Command == 7 && 
                         !string.IsNullOrEmpty(ScriptBlocks[i].Commands[j].Dialogue))
                {
                    string key = $"{Name} - BLOCK{i:D2} - COMMAND{j:D3}";
                    if (moreRecent14)
                    {
                        lines.Add($"{key} - Monologue", ScriptBlocks[i].Commands[j].Dialogue!);
                    }
                    else
                    {
                        lines.Add($"{key} - {mostRecentSpeaker}", ScriptBlocks[i].Commands[j].Dialogue!);
                    }
                }
            }
        }
        
        return JsonSerializer.Serialize(lines, _options).Replace("\\u3000", "\u3000");
    }

    public void ImportDialogueJson(string json)
    {
        var lines = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
        foreach (string key in lines.Keys)
        {
            Match match = BlockCommandRegex().Match(key);
            int blockIdx = int.Parse(match.Groups["blockIdx"].Value);
            int commandIdx = int.Parse(match.Groups["commandIdx"].Value);

            ScriptBlocks[blockIdx].Commands[commandIdx].Dialogue = lines[key];
        }
    }

    public string ExportCsv()
    {
        StringBuilder sb = new();

        for (int i = 0; i < ScriptBlocks.Count; i++)
        {
            sb.AppendLine($"Script Block {i:D2}");
            foreach (AdvPartScriptBlockCommand command in ScriptBlocks[i].Commands)
            {
                sb.AppendLine(
                    $"{command.Command},{string.Join(',', command.Parameters)}{(!string.IsNullOrEmpty(command.Dialogue) ? $",{command.Dialogue.Replace("\n", "\\n")}" : string.Empty)}");
            }
        }
        
        return sb.ToString();
    }

    [GeneratedRegex(@"BLOCK(?<blockIdx>\d{2,}) - COMMAND(?<commandIdx>\d{3,})")]
    private static partial Regex BlockCommandRegex();
}

public class AdvPartScriptBlock
{
    public List<AdvPartScriptBlockCommand> Commands { get; set; } = [];

    public AdvPartScriptBlock(byte[] data)
    {
        for (int i = 0; i < data.Length;)
        {
            string invocationString = IO.ReadShiftJisString(data, i);
            Commands.Add(new(invocationString));
            i += invocationString.GetShiftJisLength() + 1;
        }
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = [.. Commands.SelectMany(c => c.GetBytes())];
        
        bytes.InsertRange(0, IO.GetIntBytes(bytes.Count));
        
        return [.. bytes];
    }
}

public class AdvPartScriptBlockCommand
{
    public int Command { get; set; }
    public int[] Parameters { get; set; }
    public string? Dialogue { get; set; }
    public bool Trailing { get; set; }

    public AdvPartScriptBlockCommand(string invocation)
    {
        string[] components = invocation.Split(',');
        Command = int.Parse(components[0]);
        int lastComponentNewLineLoc = components[^1].IndexOf('\n');
        if (Command == 7 && lastComponentNewLineLoc > 0)
        {
            Dialogue = components[^1][(lastComponentNewLineLoc + 1)..];
            components[^1] = components[^1][..lastComponentNewLineLoc];
        }
        Parameters = components[1..].Where(p => !string.IsNullOrEmpty(p)).Select(int.Parse).ToArray();
        Trailing = string.IsNullOrEmpty(components[^1]);
    }

    public byte[] GetBytes()
    {
        return
        [
            .. ($"{Command},{string.Join(',', Parameters)}" +
                $"{(Trailing ? "," : string.Empty)}" +
                $"{(!string.IsNullOrEmpty(Dialogue) ? $"\n{Dialogue}" : string.Empty)}").GetShiftJisBytes(),
            0
        ];
    }
}

public enum Speaker
{
    Monologue = 0,
    Haruhi = 1,
    Asahina = 2,
    Nagato = 4,
    Kyon = 5,
    Koizumi = 6,
    Tsuruya = 7,
    Cop = 11,
}