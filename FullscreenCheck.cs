using System.Runtime.InteropServices;

namespace ICare;

public class FullscreenCheck {
    static Dictionary<IntPtr, RECT?> monitorFullscreen = new();
    
    
    public static bool IsAnAppFullscreen() {
        monitorFullscreen.Clear();
        EnumWindows(EvaluateMonitor, IntPtr.Zero);

        
        return monitorFullscreen.Values.Any(x => x.HasValue);
    }

    public static RECT? GetFullscreenMonitorRect() {
        var monitor = monitorFullscreen.FirstOrDefault(kvp => kvp.Value.HasValue);

        if (!monitor.Value.HasValue)
            return null;

        GetDpiForMonitor(monitor.Key, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);

        int scaledLeft = (int)(monitor.Value.Value.Left / (dpiX / 96.0));
        int scaledTop = (int)(monitor.Value.Value.Top / (dpiY / 96.0));
        int scaledRight = (int)(monitor.Value.Value.Right / (dpiX / 96.0));
        int scaledBottom = (int)(monitor.Value.Value.Bottom / (dpiY / 96.0));

        RECT scaledRect = new RECT { Left = scaledLeft, Top = scaledTop, Right = scaledRight, Bottom = scaledBottom };

        return scaledRect;

    }
    public static bool EvaluateMonitor(IntPtr hwnd, IntPtr lParam) {
        if (!IsWindowVisible(hwnd))
            return true;

        DwmGetWindowAttribute(hwnd, DWMWA_CLOAKED, out int cloaked, sizeof(int));
        if(cloaked != 0)
            return true;
        
        int style = GetWindowLong(hwnd, GWL_STYLE);
        if((style & WS_CAPTION) != 0)
            return true;
        
        if (hwnd == GetShellWindow()) 
            return true;
        
        IntPtr monitorPtr = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        
        MONITORINFO monInfo = new MONITORINFO();
        monInfo.cbSize = Marshal.SizeOf<MONITORINFO>();
        GetMonitorInfo(monitorPtr, ref monInfo);
        
        DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT rect, Marshal.SizeOf<RECT>());

        if (monInfo.rcMonitor.Right == rect.Right && monInfo.rcMonitor.Left == rect.Left &&
            monInfo.rcMonitor.Top == rect.Top && monInfo.rcMonitor.Bottom == rect.Bottom) {
            monitorFullscreen[monitorPtr] = monInfo.rcMonitor;
        }

        return true;
    }
    
    [DllImport("shcore.dll")]
    static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    const int MDT_EFFECTIVE_DPI = 0;
    
    [DllImport("user32.dll")]
    static extern IntPtr GetShellWindow();
    
    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);
    
    [DllImport("dwmapi.dll")]
    static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);
    
    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    const int GWL_STYLE = -16;
    const uint WS_CAPTION = 0x00C00000;
    
    
    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    const uint MONITOR_DEFAULTTONEAREST = 2;
    
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    struct MONITORINFO {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll")]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    
    
    [DllImport("dwmapi.dll")]
    static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

    const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
    const int DWMWA_CLOAKED = 14;
    
    
    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
}