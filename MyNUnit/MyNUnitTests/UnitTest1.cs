using MyNUnit.TestRunnerLibrary.Core;
public class Tests
{
    
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void Test_RunPassingTests_ShouldPass()
    {
      
        var testRunner = new TestRunner();
        var assemblyPath = "MyNUnit/SampleTestFail";
     
        var results = testRunner.RunTests(assemblyPath);
        
        Assert.IsNotEmpty(results);
        Assert.IsTrue(results.All(r => r.Passed));
    }
    [Test]
    public void Test_RunFailingTests_ShouldFail()
    {
        var testRunner = new TestRunner();
        var path = "MMyNUnit/MyNUnit/TestRunnerConsoleApp";
        
        var results = testRunner.RunTests(path);
        
        Assert.IsNotEmpty(results);
        Assert.IsTrue(results.All(r => !r.Passed));
    }

    [Test]
    public void Test_RunIgnoredTests_ShouldBeIgnored()
    {
        var testRunner = new TestRunner();
        var path = "MyNUnit/MyNUnit/TestRunnerConsoleApp";
        
        var results = testRunner.RunTests(path);
        
        Assert.IsNotEmpty(results);
        Assert.IsTrue(results.All(r => !string.IsNullOrEmpty(r.IgnoreReason)));
    }

}