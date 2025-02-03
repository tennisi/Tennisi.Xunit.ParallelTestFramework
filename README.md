
# Tennisi.Xunit assembly

## Tennisi.Xunit namespace

| public type | description |
| --- | --- |
| struct [ParallelTag](./Tennisi.Xunit/ParallelTag.md) | A readonly structure that serves as a `Xunit` fixture to provide unique but constant value for test fact or theory version, facilitating parallel execution of tests while ensuring consistency in tagging. |
| class [TestParallelizationAttribute](./Tennisi.Xunit/TestParallelizationAttribute.md) | An attribute that controls test parallelization in the xUnit framework. Can be applied at the assembly or test class level. |

<!-- DO NOT EDIT: generated by xmldocmd for Tennisi.Xunit.dll -->

# Tennisi.Xunit.v2.ParallelTestFramework assembly

## Tennisi.Xunit namespace

| public type | description |
| --- | --- |
| class [RetryClassAttribute](./Tennisi.Xunit/RetryClassAttribute.md) | Represents a custom xUnit attribute to specify that all test methods in the class (both facts and theories) should be retried a specified number of times. |
| class [RetryFactAttribute](./Tennisi.Xunit/RetryFactAttribute.md) | Represents a custom xUnit attribute to retry a test a specified number of times. |
| class [RetryTheoryAttribute](./Tennisi.Xunit/RetryTheoryAttribute.md) | Represents a custom xUnit attribute to retry a theory test a specified number of times. |

<!-- DO NOT EDIT: generated by xmldocmd for Tennisi.Xunit.v2.ParallelTestFramework.dll -->

By default, xUnit runs all test cases in a test class synchronously.
This package extends the default test framework to execute tests in parallel.

```shell
dotnet add package Tennisi.Xunit.ParallelTestFramework
```

By default, all tests will run in parallel.
Selectively disable parallelization by adding the `DisableParallelization` attribute to a collection or theory.

````c#
// All tests are run in parallel
public class ParallelTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);

    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test3(int value) => Thread.Sleep(2000);

    // This test runs in parallel with other tests
    // However, its test cases are run sequentially
    [Theory]
    [DisableParallelization]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test4(int value) => Thread.Sleep(2000);
}

// This collection runs in parallel with other collections
// However, its tests cases are run sequentially
[DisableParallelization]
public class SequentialTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);
}
````

Previous versions of this package relied on built in xUnit attributes instead of exposing a dedicated `DisableParallelization` attribute.
For backwards compatibility, parallelization can also be disabled by adding an explicit `Collection` attribute or `Theory` attribute with `DisableDiscoveryEnumeration` enabled.

````c#
// All tests are run in parallel
public class ParallelTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);

    [Theory]
    [InlineData(0), InlineData(1), InlineData(2)]
    public void Test3(int value) => Thread.Sleep(2000);

    // This test runs in parallel with other tests
    // However, its test cases are run sequentially because of DisableDiscoveryEnumeration
    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    public void Test4(int value) => Thread.Sleep(2000);

    public static TheoryData<int> GetData() =>  new() { { 0 }, { 1 } };
}

// This collection runs in parallel with other collections
// However, its tests cases are run sequentially because the Collection is explicit
[Collection("Sequential")]
public class SequentialTests
{
    [Fact]
    public void Test1() => Thread.Sleep(2000);

    [Fact]
    public void Test2() => Thread.Sleep(2000);
}
````

The code is greatly inspired by the sample from [Travis Mortimer](https://github.com/tmort93): <https://github.com/xunit/xunit/issues/1986#issuecomment-831322722>


## Parallel in a collection

Using the `EnableParallelizationAttribute` on an `ICollectionFixture<T>` enables the parallel execution between classes in a collection.
If you want to enable parallisation inside a class you still need to add the `EnableParallelizationAttribute` on the test class aswell.

```c#
[CollectionDefinition("MyFixture Collection")]
[EnableParallelization] // This enables the parallel execution of classes in a collection 
public class MyFixtureCollection : ICollectionFixture<MyFixture>
{
}

[Collection("MyFixture Collection")]
public class MyFirstTestClass
{
    private readonly MyFixture fixture;

    public ParallelCollectionMultiClass1AttributeTests(MyFixture fixture)
    {
        this.fixture = fixture;
    }

    //...
}

[Collection("MyFixture Collection")]
public class MySecondTestClass
{
    private readonly MyFixture fixture;

    public ParallelCollectionMultiClass1AttributeTests(MyFixture fixture)
    {
        this.fixture = fixture;
    }

    //...
}
```

## Achieving Maximum Productivity

Use the `FullTestParallelizationAttribute` on an assembly and inject `ParallelTag` into the constructor of each of your tests.

```c#
public class MyTests
{
    private readonly int _betId;
    
    public MyTests(ITestOutputHelper helper, ParallelTag tag = new())
    {
        _betId = tag.AsLong();
    }
    
    [Fact]
    public void Do()
    {
       
    }   
}
```

This strategy will allow you to run each Fact or Theory variant in parallel. Each time xUnit starts your test, a new unique but constant ParallelTag will be injected into your test class, allowing your test to work with unique data without interfering with other tests.

## WinForms/WPF Applications

STA model apps should use the `Tennisi.Xunit.ParallelTestFramework.UI` package.


