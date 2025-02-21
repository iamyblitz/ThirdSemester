using MyNUnit.TestRunnerLibrary.Attributes;
using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;

namespace MyNUnit.TestRunnerLibrary.Core
{
    /// <summary>
    /// Provides functionality to discover and run test methods in assemblies located at a specified path.
    /// </summary>
    public class TestRunner
    {
        /// <summary>
        /// Discovers and runs all test methods in assemblies located at the given path.
        /// </summary>
        /// <param name="path">The file system path where assemblies are located.</param>
        /// <returns>An enumerable collection of test results.</returns>
        public IEnumerable<TestResult> RunTests(string path)
        {
            var results = new ConcurrentBag<TestResult>();

            var assemblies = LoadAssemblies(path);

            // Get all types from the loaded assemblies
            IEnumerable<Type> allTypes = assemblies.SelectMany(a => a.GetTypes());

            // Find all classes that contain methods with the [Test] attribute
            IEnumerable<Type> testClasses = allTypes.Where(t =>
            {
                MethodInfo[] methods = t.GetMethods();

                bool hasTestMethod = methods.Any(m =>
                {
                    object[] testAttributes = m.GetCustomAttributes(typeof(TestAttribute), false);
                    return testAttributes.Any();
                });

                return hasTestMethod;
            });

            // Run tests in each class in parallel
            Parallel.ForEach(testClasses, testClass =>
            {
                RunTestsInClass(testClass, results);
            });

            return results;
        }

        /// <summary>
        /// Loads all assemblies with a .dll extension from the specified path.
        /// </summary>
        /// <param name="path">The directory path to search for assemblies.</param>
        /// <returns>An enumerable collection of loaded assemblies.</returns>
        private IEnumerable<Assembly> LoadAssemblies(string path)
        {
            var assemblyFiles = Directory.GetFiles(path, "*.dll");
            foreach (var file in assemblyFiles)
            {
                yield return Assembly.LoadFrom(file);
            }
        }

        /// <summary>
        /// Runs all static methods in the given class that are marked with the specified attribute.
        /// </summary>
        /// <param name="testClass">The type representing the test class.</param>
        /// <param name="attributeType">The attribute type to look for.</param>
        private void RunStaticMethodsWithAttribute(Type testClass, Type attributeType)
        {
            var methods = testClass.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(attributeType, false).Any());

            foreach (var method in methods)
            {
                method.Invoke(null, null);
            }
        }

        /// <summary>
        /// Runs all test methods within a given test class and collects their results.
        /// </summary>
        /// <param name="testClass">The type representing the test class.</param>
        /// <param name="results">A thread-safe collection to store test results.</param>
        private void RunTestsInClass(Type testClass, ConcurrentBag<TestResult> results)
        {
            // Run methods marked with [BeforeClass]
            RunStaticMethodsWithAttribute(testClass, typeof(BeforeClassAttribute));

            // Find all methods marked with [Test]
            var testMethods = testClass.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Any());

            // Run each test method in parallel
            Parallel.ForEach(testMethods, testMethod =>
            {
                var result = RunTestMethod(testClass, testMethod);
                results.Add(result);
            });

            // Run methods marked with [AfterClass]
            RunStaticMethodsWithAttribute(testClass, typeof(AfterClassAttribute));
        }

        /// <summary>
        /// Executes a single test method and returns the result.
        /// </summary>
        /// <param name="testClass">The type representing the test class.</param>
        /// <param name="testMethod">The method information of the test method.</param>
        /// <returns>A <see cref="TestResult"/> representing the outcome of the test.</returns>
        private TestResult RunTestMethod(Type testClass, MethodInfo testMethod)
        {
            var result = new TestResult { TestName = testMethod.Name };
            var testAttribute = (TestAttribute)testMethod.GetCustomAttribute(typeof(TestAttribute));

            // Check if the test is marked with [Test(Ignore = "reason")]
            if (!string.IsNullOrEmpty(testAttribute.Ignore))
            {
                result.IgnoreReason = testAttribute.Ignore;
                return result;
            }

            var stopwatch = Stopwatch.StartNew();

            object instance = null;
            try
            {
                // Create an instance of the test class
                instance = Activator.CreateInstance(testClass);

                // Run methods marked with [Before]
                RunInstanceMethodsWithAttribute(instance, typeof(BeforeAttribute));

                // Invoke the test method
                testMethod.Invoke(instance, null);

                // If no exception is expected, the test passes
                result.Passed = testAttribute.Expected == null;
            }
            catch (Exception ex)
            {
                var actualException = ex is TargetInvocationException ? ex.InnerException : ex;

                // Check if the thrown exception matches the expected exception
                if (testAttribute.Expected != null && actualException.GetType() == testAttribute.Expected)
                {
                    result.Passed = true;
                }
                else
                {
                    result.Passed = false;
                    result.FailureMessage = actualException.Message;
                }
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;

                if (instance != null)
                {
                    // Run methods marked with [After]
                    RunInstanceMethodsWithAttribute(instance, typeof(AfterAttribute));
                }
            }

            return result;
        }

        /// <summary>
        /// Runs all instance methods in the given object that are marked with the specified attribute.
        /// </summary>
        /// <param name="instance">The instance of the test class.</param>
        /// <param name="attributeType">The attribute type to look for.</param>
        private void RunInstanceMethodsWithAttribute(object instance, Type attributeType)
        {
            var methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(attributeType, false).Any());

            foreach (var method in methods)
            {
                method.Invoke(instance, null);
            }
        }
    }
}
