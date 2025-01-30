using Xunit;

namespace Tennisi.v3.Tests;

internal static class SeedClass
{
    private static object locker = new object();
    private static string SeedFile = "_seed.txt";
    static SeedClass()
    {
        lock (locker)
        {
            System.IO.File.WriteAllText(SeedFile, "");
        }
    }

    public static async Task Seed(string clx, int number)
    {
        await Task.Delay(100);

        lock (locker)
        {
#pragma warning disable CA1849
            File.AppendAllLines(SeedFile, new string[] { clx + number });
#pragma warning restore CA1849
        }
    }
}

public class SeedTest1
{
    [Fact]
    public void Run()
    {
        Assert.True(true);
    }

    [Fact]
    public async Task Run1()
    {
        await SeedClass.Seed("A", 1);
        Assert.True(true);
    }

    [Fact]
    public async Task Run2()
    {
        await SeedClass.Seed("A",2);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run3()
    {
        await SeedClass.Seed("A",3);
        Assert.True(true);
    }

    [Fact]
    public async Task Run4()
    {
        await SeedClass.Seed("A",4);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run5()
    {
        await SeedClass.Seed("A",5);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run6()
    {
        await SeedClass.Seed("A", 6);
        Assert.True(true);
    }

    [Fact]
    public async Task Run7()
    {
        await SeedClass.Seed("A",7);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run8()
    {
        await SeedClass.Seed("A",8);
        Assert.True(true);
    }

    [Fact]
    public async Task Run9()
    {
        await SeedClass.Seed("A",8);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run10()
    {
        await SeedClass.Seed("A",10);
        Assert.True(true);
    }
}

public class SeedTest2
{

    [Fact]
    public async Task Run11()
    {
        await SeedClass.Seed("B",11);
        Assert.True(true);
    }

    [Fact]
    public async Task Run12()
    {
        await SeedClass.Seed("B",12);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run13()
    {
        await SeedClass.Seed("B",13);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run14()
    {
        await SeedClass.Seed("B",14);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run15()
    {
        await SeedClass.Seed("B",15);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run16()
    {
        await SeedClass.Seed("B",16);
        Assert.True(true);
    }

    [Fact]
    public async Task Run17()
    {
        await SeedClass.Seed("B",17);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run18()
    {
        await SeedClass.Seed("B",18);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run19()
    {
        await SeedClass.Seed("B",19);
        Assert.True(true);
    }
    
    [Fact]
    public async Task Run20()
    {
        await SeedClass.Seed("B",20);
        Assert.True(true);
    }
}