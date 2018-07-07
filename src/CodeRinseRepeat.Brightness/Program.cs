using System;
using Mono.Options;

namespace CodeRinseRepeat.Brightness
{
    class Program
    {
        internal static Version Version => typeof(Program).Assembly.GetName().Version;
        internal static int Verbosity = 0;

        static readonly CommandSet brightnessCommandSet = new CommandSet("clibright") {
            "usage: clibright COMMAND [OPTIONS]",
            string.Empty,
            "Manipulate monitor brightness.",
            string.Empty,
            "Global options:",
            { "v|verbose", "Output verbosity. Specify multiple times to increase verbosity further.", _ => Verbosity++ },
            string.Empty,
            "Available commands:",
            new IncrementCommand(),
            new DecrementCommand(),
            new SetCommand()
        };

        static int Main(string[] args) => brightnessCommandSet.Run(args);
    }
}
