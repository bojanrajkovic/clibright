using System;
using System.Collections.Generic;
using Mono.Options;

namespace CodeRinseRepeat.Brightness
{
    internal class SetCommand : Command
    {
        bool showHelp;
        uint monitorIndex;

        public SetCommand() : base("set", "Set brightness to a percentage")
        {
            Options = new OptionSet {
                "usage: clibright set [OPTIONS] <brightness%>",
                string.Empty,
                "Set monitor brightness to a % value.",
                { "h|?|help", "Show this message and exit.", v => showHelp = v != null },
                {
                    "i|index",
                    "Physical monitor index to change brightness for. " +
                    "0 indicates all monitors, see the list command to get a monitor's index.",
                    (uint v) => monitorIndex = v
                }
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            try {
                var remaining = Options.Parse(arguments);

                if (showHelp) {
                    Options.WriteOptionDescriptions(CommandSet.Out);
                    return 0;
                }

                if (remaining.Count < 1) {
                    CommandSet.Error.WriteLine("clibright: You must specify a brightness percentage!");
                    return 1;
                }

                if (remaining.Count > 1)
                    CommandSet.Error.WriteLine("clibright: Multiple percentages specified, only using the first.");

                var percentageString = remaining[0];

                if (!float.TryParse(percentageString, out var percentage)) {
                    CommandSet.Error.WriteLine("clibright: Invalid percentage value.");
                    return 1;
                }

                if (0.0f > percentage || percentage > 100.0f) {
                    CommandSet.Error.WriteLine("clibright: Invalid range for percentage, must be between 0 and 100%.");
                    return 1;
                }
                percentage /= 100f;

                var physicalMonitors = NativeMethods.GetPhysicalMonitors();

                if (monitorIndex > physicalMonitors.Count) {
                    CommandSet.Error.WriteLine(
                        $"clibright: Invalid monitor index {monitorIndex}, there are only " +
                        $"{physicalMonitors.Count} attached physical monitors."
                    );
                    return 1;
                }

                for (var i = 0; i < physicalMonitors.Count; i++) {
                    var physicalMonitor = physicalMonitors[i];
                    if (monitorIndex == 0 || monitorIndex - 1 == i) {
                        if (Program.Verbosity > 0)
                            Console.WriteLine($"monitor {i + 1}, new percentage: {percentage:P}");

                        NativeMethods.SetMonitorBrightness(physicalMonitor, percentage);
                    }
                }

                return 0;
            } catch (Exception e) {
                CommandSet.Error.WriteLine("clibright: {0}", Program.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }
}
