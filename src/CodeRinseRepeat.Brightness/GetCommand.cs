using System;
using System.Collections.Generic;
using System.Text;
using Mono.Options;

namespace CodeRinseRepeat.Brightness
{
    internal class GetCommand : Command
    {
        bool showHelp;
        uint monitorIndex;

        public GetCommand() : base("get", "Get monitor brightness")
        {
            Options = new OptionSet {
                "usage: clibright set [OPTIONS] <brightness%>",
                string.Empty,
                "Set monitor brightness to a % value.",
                { "h|?|help", "Show this message and exit.", v => showHelp = v != null },
                {
                    "i|index:",
                    "Physical monitor index to get brightness for. " +
                    "0 indicates all monitors, see the list command to get a monitor's index.",
                    (uint v) => monitorIndex = v
                }
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            try {
                Options.Parse(arguments);

                if (showHelp) {
                    Options.WriteOptionDescriptions(CommandSet.Out);
                    return 0;
                }

                var physicalMonitors = NativeMethods.GetPhysicalMonitors();

                if (monitorIndex > physicalMonitors.Count) {
                    CommandSet.Error.WriteLine(
                        $"clibright: Invalid monitor index {monitorIndex}, there are only " +
                        $"{physicalMonitors.Count} attached physical monitors."
                    );
                    return 1;
                }

                var output = new StringBuilder();
                for (var i = 0; i < physicalMonitors.Count; i++) {
                    var physicalMonitor = physicalMonitors[i];
                    if (monitorIndex != 0 && monitorIndex - 1 != i)
                        continue;

                    NativeMethods.GetMonitorBrightness(physicalMonitor.MonitorHandle, out _, out var curBright, out var maxBright);
                    output.AppendFormat(",{0:P}", (float)curBright/maxBright);
                }

                Console.WriteLine(output.Remove(0, 1).ToString());
                return 0;
            } catch (Exception e) {
                CommandSet.Error.WriteLine("clibright: {0}", Program.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }
}
