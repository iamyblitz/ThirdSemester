

namespace MyNUnit.TestRunnerLibrary.Core;

using MyNUnit.TestRunnerLibrary.Attributes;
using System.Collections.Concurrent;
using  System.Reflection;
using System.Diagnostics;

public class TestRunner()
{
    public IEnumerable<TestResult> RunTests(string path)
    {
        var results = new ConcurrentBag<TestResult>();

        var assemblies = LoadAssemblies(path);

        var testClasses = assemblies.SelectMany(a => a.GetTypes()).Where(t =>
            t.GetMethods().Any(m => m.GetCustomAttributes(typeof(TestAttribute), false).Any()));

        Parallel.ForEach(testClasses, testClass => { RunTestsInClass(testClass, results); }); 
        return results;
    }


    private IEnumerable<Assembly> LoadAssemblies(string path)
    {
        var assemblyFiles = Directory.GetFiles(path, "*.dll");
        foreach (var file in assemblyFiles)
        {
            yield return Assembly.LoadFrom(file);
        }
    }
    private void RunStaticMethodsWithAttribute(Type testClass, Type attributeType)
    {
        var methods = testClass.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(attributeType, false).Any());

        foreach (var method in methods)
        {
            method.Invoke(null, null);
        }
    }  

    private void RunTestsInClass(Type testClass, ConcurrentBag<TestResult> results)
    {
        RunStaticMethodsWithAttribute(testClass, typeof(BeforeClassAttribute));

        var testMethods = testClass.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Any());

        Parallel.ForEach(testMethods, testMethod =>
        {
            var result = RunTestMethod(testClass, testMethod);
            results.Add(result);
        });

       
        RunStaticMethodsWithAttribute(testClass, typeof(AfterClassAttribute));
    }
    private TestResult RunTestMethod(Type testClass, MethodInfo testMethod)
    {
        var result = new TestResult { TestName = testMethod.Name };
        var testAttribute = (TestAttribute)testMethod.GetCustomAttribute(typeof(TestAttribute));
    
        if (!string.IsNullOrEmpty(testAttribute.Ignore))
        {
            result.IgnoreReason = testAttribute.Ignore;
            return result;
        }
    
        var stopwatch = Stopwatch.StartNew();
    
        object instance = null;
        try
        {
            instance = Activator.CreateInstance(testClass);
    
           
            RunInstanceMethodsWithAttribute(instance, typeof(BeforeAttribute));
    
           
            testMethod.Invoke(instance, null);
            result.Passed = testAttribute.Expected == null;
        }
        catch (Exception ex)
        {
            var actualException = ex is TargetInvocationException ? ex.InnerException : ex;
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
                RunInstanceMethodsWithAttribute(instance, typeof(AfterAttribute));
            }
        }
    
        return result;
    }
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


  



