using System.Runtime.InteropServices;

namespace ICare;

public class IdleTimeDetector {
    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public static TimeSpan GetIdleTime() {
        var lastInputInfo = new LASTINPUTINFO {
            cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
        };

        if (!GetLastInputInfo(ref lastInputInfo))
            throw new InvalidOperationException("GetLastInputInfo failed.");

        uint idleTicks = (uint)Environment.TickCount - lastInputInfo.dwTime;
        return TimeSpan.FromMilliseconds(idleTicks);
    }
}