using Microsoft.Win32;

namespace ICare;

public class AppInfo {
    public const string Name = "ICare";
    
    public static Theme SystemTheme => GetTheme();
    public static Version Version => ((App)System.Windows.Application.Current).Version.Version;

    private static Theme GetTheme() {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string valueName = "AppsUseLightTheme";

        using var key = Registry.CurrentUser.OpenSubKey(keyPath);
        var value = key?.GetValue(valueName);
        
        if (value == null || (int) value == 1)
            return Theme.Light;
        
        return Theme.Dark;
    }
}