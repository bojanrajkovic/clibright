using System;
using System.Collections.Generic;
using Mono.Options;

namespace CodeRinseRepeat.Brightness
{
    class DecrementCommand : Command
    {
        bool showHelp;
        uint monitorIndex;

        public DecrementCommand() : base("decrement", "Decrement brightness by a given percentage")
        {
            Options = new OptionSet {
                "usage: clibright decrement [OPTIONS] <decrement>",
                string.Empty,
                "Decrement the current monitor brightness percentage by the given value.",
                { "h|?|help", "Show this message and exit.", v => showHelp = v != null },
                {
                    "i|index:",
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
                    CommandSet.Error.WriteLine("clibright: You must specify a decrement!");
                    return 1;
                }

                if (remaining.Count > 1)
                    CommandSet.Error.WriteLine("clibright: Multiple decrements specified, only using the first.");

                var decrementString = remaining[0];
                if (!int.TryParse(decrementString, out var decrement)) {
                    CommandSet.Error.WriteLine("clibright: Invalid decrement value.");
                    return 1;
                }

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

                    if (monitorIndex != 0 && monitorIndex - 1 != i)
                        continue;

                    NativeMethods.GetMonitorBrightness(
                        physicalMonitor.MonitorHandle,
                        out _,
                        out short currentBrightness,
                        out short maximumBrightness
                    );
                    var currentPercentage = (float)currentBrightness / maximumBrightness;
                    var newPercentage = Math.Max(currentPercentage - decrement / 100f, 0.0f);

                    if (Program.Verbosity > 0)
                        Console.WriteLine($"monitor {i+1}, current percentage: {currentPercentage:P}, new percentage: {newPercentage:P}");

                    NativeMethods.SetMonitorBrightness(physicalMonitor, newPercentage);
                }

                return 0;
            } catch (Exception e) {
                CommandSet.Error.WriteLine("clibright: {0}", Program.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }
}
