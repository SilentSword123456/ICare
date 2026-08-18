using System.Windows.Media.Imaging;
using ICare.Models;

namespace ICare.Design;

public class DesignApps {
    public List<InstalledApp> Apps { get; } = new() {
        new InstalledApp {
            Name = "Sample App",
            Icon = new BitmapImage(new System.Uri("file:///C:/Users/SilentSword/Downloads/test.png"))
        }
    };
}