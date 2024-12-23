using Tennisi.Xunit.UIParallelTestFramework.Tests.Forms;
using Xunit;

namespace Tennisi.Xunit.UIParallelTestFramework.Tests;

public class TestWindowTests
{
    [StaFact]
    public void ItShouldClick()
    {
        var window = new TestWindow();
        
        var button = window.PublicButton;
        
        Assert.NotNull(button);
        Assert.Equal("Click Me", button.Content);
    }
    
    [StaTheory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void ItShouldClickTheory(int a)
    {
        var window = new TestWindow();
        
        var button = window.PublicButton;
        button.Content = a;
        Assert.NotNull(button);
        Assert.Equal(a, button.Content);
    }
    
    [Fact]
    public void ItShouldSomeNonStaAction()
    {
        Assert.True(true); //
    }
    
    [StaTheory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public async Task ItShouldRunLongRunning(int a)
    {
        await Task.Delay(1000 + a);
    }
}