using System;
using System.Collections.Generic;
using Mono.Options;

namespace CodeRinseRepeat.Brightness
{
    class IncrementCommand : Command
    {
        bool showHelp;
        uint monitorIndex;

        public IncrementCommand() : base("increment", "Increment brightness by a percentage")
        {
            Options = new OptionSet {
                "usage: clibright increment [OPTIONS] <increment>",
                string.Empty,
                "Increment the current monitor brightness percentage by the given value.",
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
                    CommandSet.Error.WriteLine("clibright: You must specify an increment!");
                    return 1;
                }

                if (remaining.Count > 1)
                    CommandSet.Error.WriteLine("clibright: Multiple increments specified, only using the first.");

                var incrementString = remaining[0];
                if (!int.TryParse(incrementString, out var increment)) {
                    CommandSet.Error.WriteLine("clibright: Invalid percentage value.");
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
                        out var currentBrightness,
                        out var maximumBrightness
                    );
                    var currentPercentage = (float)currentBrightness / maximumBrightness;
                    var newPercentage = Math.Min(currentPercentage + increment / 100f, 1f);

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
