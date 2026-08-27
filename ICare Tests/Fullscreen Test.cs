using NUnit.Framework;
using ICare;

namespace ICare_Tests;

public class Tests {
    [SetUp]
    public void Setup() {
    }

    [Test]
    public void Debug_FullscreenCheck() {
        var result = FullscreenCheck.IsAnAppFullscreen();
        TestContext.WriteLine($"IsAnAppFullscreen: {result}");
    }
}