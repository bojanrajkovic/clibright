using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Newtonsoft.Json;
using MonitorInfoCollection = System.Collections.Generic.List<(
    System.IntPtr monitorHandle,
    CodeRinseRepeat.Brightness.NativeStructures.MonitorInfoEx monitorInfo,
    CodeRinseRepeat.Brightness.NativeStructures.PhysicalMonitor[] physicalMonitors
)>;

using static CodeRinseRepeat.Brightness.NativeMethods;
using static CodeRinseRepeat.Brightness.NativeStructures;
using System.Drawing;

namespace CodeRinseRepeat.Brightness
{
    internal class ListCommand : Command
    {
        string format = "human";
        bool showHelp;

        public ListCommand() : base("list", "Lists attached monitors")
        {
            Options = new OptionSet {
                "usage: clibright list [OPTIONS]",
                string.Empty,
                "List attached monitors.",
                { "h|?|help", "Show this message and exit.", v => showHelp = v != null },
                { "f|format:", "The {format} to output the data in. Accepted values: human, json. Defaults to human.", v => format = v }
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

                if (format != "human" && format != "json") {
                    CommandSet.Error.WriteLine($"Invalid format {format}, the only acceptable values are \"human\" and \"json\".");
                    return 1;
                }

                var info = new MonitorInfoCollection();
                bool EnumMonitorsCallback(IntPtr monitor, IntPtr hdcMonitor, ref NativeStructures.Rect lprcMonitor, IntPtr data)
                {
                    var monitorInfo = new MonitorInfoEx(0);
                    if (!GetMonitorInfo(monitor, ref monitorInfo)) {
                        Console.Error.WriteLine("Could not get monitor info for monitor 0x{0:X}.", monitor.ToInt64());
                        return true;
                    }

                    GetNumberOfPhysicalMonitorsFromHMonitor(monitor, out var numberOfMonitors);

                    var physicalMonitors = new PhysicalMonitor[numberOfMonitors];
                    GetPhysicalMonitorsFromHMonitor(monitor, numberOfMonitors, physicalMonitors);

                    info.Add((monitor, monitorInfo, physicalMonitors));
                    return true;
                }

                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, EnumMonitorsCallback, IntPtr.Zero);

                switch (format) {
                    case "human":
                        PrintHumanReadableMonitorInfo(info);
                        break;
                    case "json":
                        PrintJsonInfo(info);
                        break;
                }

                return 0;
            } catch (Exception e) {
                CommandSet.Error.WriteLine("clibright: {0}", Program.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }

        static void PrintJsonInfo(MonitorInfoCollection info)
        {
            var counter = 1;
            var jsonObject = info.Select(tup => new {
                MonitorName = tup.monitorInfo.DeviceName,
                Handle = tup.monitorHandle.ToString("X"),
                MonitorSize = new Size(
                    tup.monitorInfo.Monitor.Right - tup.monitorInfo.Monitor.Left,
                    tup.monitorInfo.Monitor.Bottom - tup.monitorInfo.Monitor.Top
                ),
                MonitorLocation = new Point(
                    tup.monitorInfo.Monitor.Left,
                    tup.monitorInfo.Monitor.Top
                ),
                PhysicalMonitors = tup.physicalMonitors.Select(pm => {
                    GetMonitorBrightness(pm.MonitorHandle, out var minBright, out var curBright, out var maxBright);
                    return new {
                        MonitorIndex = counter++,
                        MinimumBrightness = minBright,
                        CurrentBrightness = curBright,
                        MaximumBrightness = maxBright
                    };
                })
            });
            Console.WriteLine(JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
        }

        static void PrintHumanReadableMonitorInfo(MonitorInfoCollection info)
        {
            var counter = 1;
            foreach (var (monitorHandle, monitorInfo, physicalMonitors) in info) {
                Console.WriteLine($"Found monitor {monitorInfo.DeviceName}, and is located at {monitorInfo.Monitor}");
                Console.WriteLine($"It has {physicalMonitors.Length} physical monitors:");
                foreach (var physicalMonitor in physicalMonitors) {
                    Console.WriteLine($"    Physical monitor {counter++}, name {physicalMonitor.PhysicalMonitorDescription}");
                    GetMonitorBrightness(physicalMonitor.MonitorHandle, out var minBright, out var curBright, out var maxBright);
                    Console.WriteLine($"        Current brightness: {curBright} - Minimum brightness: {minBright} - Maximum brightness: {maxBright}");
                }
            }
        }
    }
}
