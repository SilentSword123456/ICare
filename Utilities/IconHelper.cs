using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ICare.Utilities;

public class IconHelper {
    public static ImageSource? GetIconForExe(string exePath) {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return null;

        using Icon? icon = Icon.ExtractAssociatedIcon(exePath);
        if (icon == null)
            return null;

        ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        imageSource.Freeze();

        return imageSource;
    }
}