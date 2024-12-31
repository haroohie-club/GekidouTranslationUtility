using Mono.Options;

namespace HaruhiGekidouCLI;

class Program
{
    static void Main(string[] args)
    {
        CommandSet commands = new("HaruhiGekidouCLI")
        {
            new ScriptArcCommand(),
        };

        commands.Run(args);
    }
}