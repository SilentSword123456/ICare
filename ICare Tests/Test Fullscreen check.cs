using ICare;
using NUnit.Framework;

namespace ICare_Tests;

public class Test_Fullscreen_check {
    
    [Test]
    public void Debug_FullscreenCheck() {
        var result = FullscreenCheck.IsAnAppFullscreen();
        TestContext.WriteLine($"IsAnAppFullscreen: {result}");
    }
}