using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Reflectors;

public static class Reflector
{
    /// <summary>
    /// Creates a .cs file describing the structure of the given class.
    /// </summary>
    /// <param name="someClass">The Type of the class to describe.</param>
    public static void PrintStructure(Type someClass)
    {
        if (!someClass.IsClass)
        {
            throw new ArgumentException("The provided type is not a class.");
        }

        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine(GetClassDeclaration(someClass));
        sb.AppendLine("{");
        
        foreach (var field in someClass.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            sb.AppendLine("    " + GetFieldDeclaration(field));
        }
        
        foreach (var method in someClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            sb.AppendLine("    " + GetMethodDeclaration(method));
        }

       
        foreach (var nestedType in someClass.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            sb.AppendLine();
            sb.AppendLine(GetInnerClassDeclaration(nestedType, "    "));
        }

        sb.AppendLine("}");
        
        File.WriteAllText($"{someClass.Name}.cs", sb.ToString());
    }

    /// <summary>
    /// Outputs all fields and methods that differ between two classes.
    /// </summary>
    /// <param name="a">The Type of the first class.</param>
    /// <param name="b">The Type of the second class.</param>
    public static void DiffClasses(Type a, Type b)
    {
        if (!a.IsClass || !b.IsClass)
        {
            throw new ArgumentException("Both types must be classes.");
        }
        
        var membersA = GetMembersSignatures(a);
        var membersB = GetMembersSignatures(b);
        
        var onlyInA = membersA.Except(membersB);
        
        var onlyInB = membersB.Except(membersA);
        
        Console.WriteLine($"Members only in {a.Name}:");
        foreach (var member in onlyInA)
        {
            Console.WriteLine("  " + member);
        }
        Console.WriteLine("");
        Console.WriteLine($"Members only in {b.Name}:");
        foreach (var member in onlyInB)
        {
            Console.WriteLine("  " + member);
        }
    }
    

    private static string GetClassDeclaration(Type type)
    {
        string modifiers = GetAccessModifier(type);

        if (type.IsAbstract && type.IsSealed)
            modifiers += " static";
        else if (type.IsAbstract)
            modifiers += " abstract";
        else if (type.IsSealed)
            modifiers += " sealed";

        string className = GetTypeName(type);

        return $"{modifiers} class {className}";
    }

    private static string GetInnerClassDeclaration(Type type, string indent)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(indent + GetClassDeclaration(type));
        sb.AppendLine(indent + "{");
        
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            sb.AppendLine(indent + "    " + GetFieldDeclaration(field));
        }
        
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            sb.AppendLine(indent + "    " + GetMethodDeclaration(method));
        }

        sb.AppendLine(indent + "}");
        return sb.ToString();
    }

    private static string GetFieldDeclaration(FieldInfo field)
    {
        string modifiers = GetAccessModifier(field);

        if (field.IsStatic)
            modifiers += " static";

        string fieldType = GetTypeName(field.FieldType);
        return $"{modifiers} {fieldType} {field.Name};";
    }

    private static string GetMethodDeclaration(MethodInfo method)
    {
        string modifiers = GetAccessModifier(method);

        if (method.IsStatic)
            modifiers += " static";
        if (method.IsAbstract)
            modifiers += " abstract";
        else if (method.IsVirtual)
            modifiers += " virtual";

        string returnType = GetTypeName(method.ReturnType);
        string methodName = method.Name;
        
        string parameters = string.Join(", ", method.GetParameters()
            .Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"));

        return $"{modifiers} {returnType} {methodName}({parameters});";
    }

    private static string GetAccessModifier(MemberInfo member)
    {
        if (member is Type type)
        {
            if (type.IsPublic)
                return "public";
            if (type.IsNestedFamily)
                return "protected";
            if (type.IsNestedAssembly)
                return "internal";
            if (type.IsNestedPrivate)
                return "private";
        }
        else if (member is MethodBase method)
        {
            if (method.IsPublic)
                return "public";
            if (method.IsFamily)
                return "protected";
            if (method.IsAssembly)
                return "internal";
            if (method.IsPrivate)
                return "private";
        }
        else if (member is FieldInfo field)
        {
            if (field.IsPublic)
                return "public";
            if (field.IsFamily)
                return "protected";
            if (field.IsAssembly)
                return "internal";
            if (field.IsPrivate)
                return "private";
        }

        return "private";
    }

    private static string GetTypeName(Type type)
    {
        return type.Name;
    }

    private static string[] GetMembersSignatures(Type type)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Select(f => GetFieldDeclaration(f));

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Select(m => GetMethodDeclaration(m));

        return fields.Concat(methods).ToArray();
    }
}
