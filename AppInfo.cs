using Microsoft.Win32;

namespace ICare;

public class AppInfo {
    public const string Name = "ICare";
    
    public static bool IsLightTheme => GetIsLightTheme();

    private static bool GetIsLightTheme() {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string valueName = "AppsUseLightTheme";

        using var key = Registry.CurrentUser.OpenSubKey(keyPath);
        var value = key?.GetValue(valueName);

        return value == null || (int) value == 1;
    }
}