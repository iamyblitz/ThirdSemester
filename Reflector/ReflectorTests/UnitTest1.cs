namespace Reflectors;

public class ReflectorTests
{
    private class TestClassA
    {
        public int Field1;
        private string Field2;

        public void Method1() { }
        private int Method2(int x) { return x; }

        public class NestedClass<T>
        {
            public T GenericField;
        }
    }
    
    [Test]
    public void Test_PrintStructure_CreatesFile()
    {
       
        Type testType = typeof(TestClassA);
        string expectedFilePath = $"{testType.Name}.cs";
        
        Reflector.PrintStructure(testType);
        
        Assert.IsTrue(File.Exists(expectedFilePath), "The output file was not created.");
        
        File.Delete(expectedFilePath);
    }
    
}
