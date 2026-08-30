using ICare;
using ICare.Models;
using Microsoft.Testing.Platform.Extensions.Messages;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ICare_Tests;

public class Test_Config {
    private Config config;
    [SetUp]
    public void Setup() {
        config = new Config();
        config.Autosave = false;
    }

    [Test]
    public void TestLoad() => config.Load();

    [Test]
    public void TestWorkSec() {
        config.WorkSec = 60;

        Assert.That(config.WorkSec, Is.EqualTo(60));
    }
    
    //Now I dont wanan manually write the tests for them all

    [Test]
    public void TestAppsFolder() {
        AppsFolder appsFolder = new AppsFolder();
        appsFolder.Apps.Add(new InstalledApp(){Name = "TEST APP", ExePath = "cmd.exe"});
        appsFolder.Name = "TEST FOLDER";
        appsFolder.ShowOverlayWarning = true;
    
        config.AppsFolders.Add(appsFolder);
    
        Config newConfig = new Config();
        newConfig.Load();

        Assert.That(newConfig.AppsFolders.Count, Is.EqualTo(config.AppsFolders.Count));

        for (int i = 0; i < config.AppsFolders.Count; i++) {
            var original = config.AppsFolders[i];
            var loaded = newConfig.AppsFolders[i];

            Assert.That(loaded.Name, Is.EqualTo(original.Name));
            Assert.That(loaded.SendNotification, Is.EqualTo(original.SendNotification));
            Assert.That(loaded.ShowOverlayWarning, Is.EqualTo(original.ShowOverlayWarning));

            Assert.That(loaded.Apps.Count, Is.EqualTo(original.Apps.Count));

            for (int j = 0; j < original.Apps.Count; j++) {
                Assert.That(loaded.Apps[j].Name, Is.EqualTo(original.Apps[j].Name));
                Assert.That(loaded.Apps[j].ExePath, Is.EqualTo(original.Apps[j].ExePath));
            }
        }
    }
}