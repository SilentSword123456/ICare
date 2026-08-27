using System.Windows.Threading;
using NUnit.Framework;
using ICare;
using ICare.Views;

namespace ICare_Tests;

[Apartment(ApartmentState.STA)]
public class Tests {
    private Config config;
    private BreakOverlayWindow overlay;
    private CancellationTokenSource cts;
    
    [SetUp]
    public void Setup() {
        config = new Config();
        config.Autosave = false;
        overlay = new BreakOverlayWindow(config);
        
        cts =  new CancellationTokenSource();
        
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
    }

    [Test]
    public void Debug_FullscreenCheck() {
        var result = FullscreenCheck.IsAnAppFullscreen();
        TestContext.WriteLine($"IsAnAppFullscreen: {result}");
    }

    [Test]
    public void DisplayBreakWarningOverlay() {
        var dispatcher = Dispatcher.CurrentDispatcher;
        var task = overlay.StartBreakWarning(TimeSpan.FromSeconds(5), cts.Token, "DEBUG OVERLAY");

        var frame = new DispatcherFrame();
        task.ContinueWith(_ => dispatcher.BeginInvoke(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);

        task.GetAwaiter().GetResult();
    }
    
    [TearDown]
    public void TearDown() {
        cts.Dispose();
    }
}