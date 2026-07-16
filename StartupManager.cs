using System.Runtime.InteropServices;
using ICare;
using Microsoft.Win32;

public static class StartupManager {
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    public const string ApprovedKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    public static void Enable() {
        if (IsInRun() && IsApproved())
            return;
        
        string exePath = Environment.ProcessPath!;
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, writable: true);
        key?.SetValue(AppInfo.Name, $"\"{exePath}\"");
    }

    public static void Disable() {
        if (!IsInRun())
            return;
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, writable: true);
        key?.DeleteValue(AppInfo.Name, throwOnMissingValue: false);
    }

    public static bool IsInRun() {
        using var runKey = Registry.CurrentUser.OpenSubKey(RegistryKey);
        bool inRun = runKey?.GetValue(AppInfo.Name) != null;
        
        return inRun;
    }

    public static bool IsApproved() {
        if (!IsInRun())
            return false;
        using var approvedKey = Registry.CurrentUser.OpenSubKey(ApprovedKeyPath);
        var approvedValue = approvedKey?.GetValue(AppInfo.Name) as byte[];
        bool approvedDisabled = approvedValue is { Length: > 0 } && approvedValue[0] != 0x02 && approvedValue[0] != 0x06;
        
        return !approvedDisabled;
    }
    
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree, int dwNotifyFilter, IntPtr e, bool fAsynchronous);
}