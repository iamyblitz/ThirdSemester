namespace Reflectors;

using System;

public class TestClassA
{
    public int Field1;
    private string Field2;

    public void Method1() { }
    private int Method2(int x) { return x; }

    public class Class<T>
    {
        public T GenericField;
    }
}

public class TestClassB
{
    public int Field1;
    private string Field3;

    public void Method1() { }
    private int Method3(int y) { return y; }
    private int Method4(int y, int x) { return y+x; }

    public class Class<T, C>
    {
        public T GenericField1;
        public C GenericField2;
    }
}

class Program
{
    static void Main()
    {
        
        Reflector.PrintStructure(typeof(TestClassA));
        Reflector.PrintStructure(typeof(TestClassB));
        
        Console.WriteLine("Differences between TestClassA and TestClassB:");
        Reflector.DiffClasses(typeof(TestClassA), typeof(TestClassB));
    }
}
