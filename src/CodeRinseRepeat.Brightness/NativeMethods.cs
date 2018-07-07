using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using static CodeRinseRepeat.Brightness.NativeStructures;

namespace CodeRinseRepeat.Brightness
{
    static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(
            IntPtr hdc,
            IntPtr lprcClip,
            EnumMonitorsCallback lpfnEnum,
            IntPtr dwData
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMonitor(IntPtr hMonitor, out uint numberOfMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        public static extern bool GetPhysicalMonitorsFromHMonitor(
            IntPtr hMonitor,
            uint dwPhysicalMonitorArraySize,
            [Out] PhysicalMonitor[] pPhysicalMonitorArray
        );

        [DllImport("dxva2.dll")]
        public static extern bool SetMonitorBrightness(IntPtr hMonitor, short brightness);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorBrightness(
            IntPtr hMonitor,
            out short pdwMinimumBrightness,
            out short pdwCurrentBrightness,
            out short pdwMaximumBrightness
        );

        public static IReadOnlyList<PhysicalMonitor> GetPhysicalMonitors()
        {
            var physicalMonitors = new List<PhysicalMonitor>();

            bool EnumMonitorsCallback(IntPtr monitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr data)
            {
                GetNumberOfPhysicalMonitorsFromHMonitor(monitor, out var numberOfMonitors);

                var innerPhysicalMonitors = new PhysicalMonitor[numberOfMonitors];
                GetPhysicalMonitorsFromHMonitor(monitor, numberOfMonitors, innerPhysicalMonitors);

                physicalMonitors.AddRange(innerPhysicalMonitors);
                return true;
            }

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, EnumMonitorsCallback, IntPtr.Zero);

            return physicalMonitors.AsReadOnly();
        }

        public static void SetMonitorBrightness(PhysicalMonitor physicalMonitor, float percentage)
        {
            GetMonitorBrightness(physicalMonitor.MonitorHandle, out _, out _, out var maximumBrightness);
            var newBrightness = (short)(percentage * maximumBrightness);

            if (Program.Verbosity > 0)
                Console.WriteLine($"setting brightness of {physicalMonitor.MonitorHandle.ToString("X")} to {(newBrightness/100f):P}");

            SetMonitorBrightness(physicalMonitor.MonitorHandle, newBrightness);
        }
    }

    static class NativeStructures
    {
        const int DeviceNameSize = 32;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MonitorInfoEx
        {
            public int Size;
            public Rect Monitor;
            public Rect WorkArea;
            public uint Flags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DeviceNameSize)]
            public string DeviceName;

            public MonitorInfoEx(int _ = 0)
            {
                Size = Marshal.SizeOf<MonitorInfoEx>();
                Monitor = WorkArea = new Rect();
                Flags = 0;
                DeviceName = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            /// <summary>
            /// The x-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int Left;

            /// <summary>
            /// The y-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int Top;

            /// <summary>
            /// The x-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int Right;

            /// <summary>
            /// The y-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int Bottom;

            public override string ToString() => $"Position: ({Left}, {Top}), Size: ({Right - Left}, {Bottom - Top})";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PhysicalMonitor
        {
            public IntPtr MonitorHandle;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string PhysicalMonitorDescription;
        }
    }

    delegate bool EnumMonitorsCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);
}
