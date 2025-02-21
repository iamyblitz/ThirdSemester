namespace MyNUnit.TestRunnerLibrary.Core
{
    /// <summary>
    /// Provides functionality to generate and print test execution reports.
    /// </summary>
    public class ReportGenerator
    {
        /// <summary>
        /// Prints the test execution report to the console.
        /// </summary>
        /// <param name="results">An enumerable collection of test results to include in the report.</param>
        public void PrintReport(IEnumerable<TestResult> results)
        {
            foreach (var result in results)
            {
                if (!string.IsNullOrEmpty(result.IgnoreReason))
                {
                    Console.WriteLine($"Тест {result.TestName} пропущен: {result.IgnoreReason}");
                }
                else if (result.Passed)
                {
                    Console.WriteLine($"Тест {result.TestName} пройден за {result.ExecutionTime.TotalMilliseconds} мс");
                }
                else
                {
                    Console.WriteLine($"Тест {result.TestName} провален за {result.ExecutionTime.TotalMilliseconds} мс");
                    Console.WriteLine($"Причина: {result.FailureMessage}");
                }
            }
        }
    }
}