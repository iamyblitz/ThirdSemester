namespace MyNUnit.TestRunnerLibrary.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute
{
    public Type Expected { get; set; }
    public string Ignore { get; set; }
}