namespace MyNUnit.TestRunnerLibrary
{
    /// <summary>
    /// Represents the result of executing a test method.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Gets or sets the name of the test method.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Gets or sets the time taken to execute the test.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test passed.
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Gets or sets the reason why the test was ignored, if applicable.
        /// </summary>
        public string IgnoreReason { get; set; }

        /// <summary>
        /// Gets or sets the failure message if the test failed.
        /// </summary>
        public string FailureMessage { get; set; }
    }
}