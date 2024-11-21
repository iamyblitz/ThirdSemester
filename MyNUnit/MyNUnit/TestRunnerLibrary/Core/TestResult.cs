namespace MyNUnit.TestRunnerLibrary;

public class TestResult
{
    public string TestName { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public bool Passed { get; set; }
    public string IgnoreReason { get; set; }
    public string FailureMessage { get; set; }
}