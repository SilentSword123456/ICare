using ICare;
using ICare.Models;
using NUnit.Framework;
using Timer = ICare.Timer;

namespace ICare_Tests;

public class TestAppsMonitor {
    private Config config;
    private AppsMonitor appsMonitor;
    
    [SetUp]
    public void Setup() {
        config = new Config();
        config.Autosave = false;
        config.Load();
        
        AppsFolder appsFolder = new AppsFolder();
        appsFolder.Apps.Add(new InstalledApp(){Name = "Cmd", ExePath = "cmd.exe"});
        appsFolder.Apps.Add(new InstalledApp(){Name = "rider", ExePath = "C:\\Program Files\\JetBrains\\JetBrains Rider 2026.2.0.2\\bin\\rider64.exe"});
        appsFolder.Name = "TEST Apps Monitor";
        appsFolder.TrackTime = true;
        appsFolder.ShowOverlayWarning = true;
    
        config.AppsFolders.Add(appsFolder);
        
        appsMonitor = new AppsMonitor(config, new Timer(config, new Keyboard(config), new MeetingTracker(new AudioActivityDetector())));
        appsMonitor.Start();
    }

    [Test]
    public void TestAddingApp() {
        config.AppsFolders[0].Apps.Add(new InstalledApp(){Name = "Powershell", ExePath = "powershell.exe"});
    }

    [TearDown]
    public void TearDown() {
        appsMonitor.Stop();
    }
}