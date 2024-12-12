using Xunit;
using Tennisi.Xunit.ParallelTestFramework.UI.Tests.Forms;


namespace Tennisi.Xunit.ParallelTestFramework.UI.Tests;

public class TestWindowTests
{
    [StaFact]
    public void ItShouldClick()
    {

        var window = new TestWindow();

        // Act
        var button = window.PublicButton;

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Click Me", button.Content);
    }
}