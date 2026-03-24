using System.Collections.Immutable;
using System.Reflection;
using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class DataMethodBodyBuildersTests
{
    [Test]
    public void BuildMethodSource_SimpleReturn_GeneratesCorrectSource()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public partial class MyClass
                                                         {
                                                             public partial string GetValue();
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSimpleReturnBody data = new DataSimpleReturnBody("hello");
        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Contain("return \"hello\";"));
        Assert.That(source, Does.Contain("partial string GetValue()"));
        Assert.That(source, Does.Contain("namespace TestNamespace;"));
    }

    [Test]
    public void BuildMethodSource_SimpleReturnVoid_GeneratesEmptyBody()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public partial class MyClass
                                                         {
                                                             public partial void DoSomething();
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "DoSomething");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSimpleReturnBody data = new DataSimpleReturnBody(null);
        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Not.Contain("return"));
        Assert.That(source, Does.Contain("partial void DoSomething()"));
    }

    [Test]
    public void BuildMethodSource_SimpleReturnBool_FormatsBoolLiteral()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static partial class MyClass
                                                         {
                                                             public static partial bool IsEnabled();
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "IsEnabled");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSimpleReturnBody data = new DataSimpleReturnBody("True");
        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Contain("return true;"));
    }

    [Test]
    public void BuildMethodSource_SwitchBody_GeneratesCorrectSwitch()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static partial class MyClass
                                                         {
                                                             public static partial int GetValue(int key);
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSwitchBody data = new DataSwitchBody(
            Cases: new List<DataSwitchCase>
            {
                new DataSwitchCase(1, "10"),
                new DataSwitchCase(2, "20")
            },
            DefaultCase: new DataSwitchDefaultCase("0"));

        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Contain("switch (key)"));
        Assert.That(source, Does.Contain("case 1: return 10;"));
        Assert.That(source, Does.Contain("case 2: return 20;"));
        Assert.That(source, Does.Contain("default: return 0;"));
    }

    [Test]
    public void BuildMethodSource_SwitchBody_WithThrowDefault_GeneratesThrowStatement()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static partial class MyClass
                                                         {
                                                             public static partial int GetValue(int key);
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSwitchBody data = new DataSwitchBody(
            Cases: new List<DataSwitchCase>(),
            DefaultCase: new DataSwitchDefaultCase("throw new ArgumentException(\"bad\")"));

        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Contain("default: throw new ArgumentException(\"bad\");"));
        Assert.That(source, Does.Not.Contain("default: return throw"));
    }

    [Test]
    public void BuildMethodSource_SwitchBody_WithNoParameters_UsesDefaultExpressionOnly()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static partial class MyClass
                                                         {
                                                             public static partial string GetValue();
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSwitchBody data = new DataSwitchBody(
            Cases: new List<DataSwitchCase> { new DataSwitchCase(1, "\"one\"") },
            DefaultCase: new DataSwitchDefaultCase("\"fallback\""));

        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Contain("return \"fallback\";"));
        Assert.That(source, Does.Not.Contain("switch"));
    }

    [Test]
    public void BuildMethodSource_SwitchBody_WithBoolKeys_FormatsBoolLiterals()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static partial class MyClass
                                                         {
                                                             public static partial string GetLabel(bool flag);
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetLabel");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSwitchBody data = new DataSwitchBody(
            Cases: new List<DataSwitchCase>
            {
                new DataSwitchCase(true, "\"Yes\""),
                new DataSwitchCase(false, "\"No\"")
            },
            DefaultCase: new DataSwitchDefaultCase("\"Unknown\""));

        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Contain("case true: return \"Yes\";"));
        Assert.That(source, Does.Contain("case false: return \"No\";"));
        Assert.That(source, Does.Contain("default: return \"Unknown\";"));
    }

    [Test]
    public void BuildMethodSource_SwitchBody_WithoutDefault_OmitsDefaultClause()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static partial class MyClass
                                                         {
                                                             public static partial int GetValue(int key);
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSwitchBody data = new DataSwitchBody(
            Cases: new List<DataSwitchCase>
            {
                new DataSwitchCase(1, "10")
            },
            DefaultCase: null);

        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.Contain("case 1: return 10;"));
        Assert.That(source, Does.Not.Contain("default:"));
    }

    [Test]
    public void BuildMethodSource_IncludesAutoGeneratedHeader()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public partial class MyClass
                                                         {
                                                             public partial string GetValue();
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        INamedTypeSymbol containingType = partialMethod.ContainingType;

        DataSimpleReturnBody data = new DataSimpleReturnBody("hello");
        string source = DataMethodBodyBuilders.BuildMethodSource(data, partialMethod, containingType);

        Assert.That(source, Does.StartWith("// <auto-generated/>"));
        Assert.That(source, Does.Contain("#pragma warning disable"));
    }

    [Test]
    public void FormatValueAsCSharpLiteral_String_QuotesCorrectly()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public partial class MyClass
                                                         {
                                                             public partial string GetValue();
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        string result = DataMethodBodyBuilders.FormatValueAsCSharpLiteral("hello", partialMethod.ReturnType);

        Assert.That(result, Is.EqualTo("\"hello\""));
    }

    [Test]
    public void FormatValueAsCSharpLiteral_Null_ReturnsDefault()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public partial class MyClass
                                                         {
                                                             public partial string GetValue();
                                                         }
                                                         """);

        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.MyClass", "GetValue");
        string result = DataMethodBodyBuilders.FormatValueAsCSharpLiteral(null, partialMethod.ReturnType);

        Assert.That(result, Is.EqualTo("default"));
    }

    [Test]
    public void FormatKeyAsCSharpLiteral_Bool_ReturnsLowerCase()
    {
        string trueResult = DataMethodBodyBuilders.FormatKeyAsCSharpLiteral(true, null);
        string falseResult = DataMethodBodyBuilders.FormatKeyAsCSharpLiteral(false, null);

        Assert.That(trueResult, Is.EqualTo("true"));
        Assert.That(falseResult, Is.EqualTo("false"));
    }

    [Test]
    public void FormatKeyAsCSharpLiteral_String_QuotesCorrectly()
    {
        string result = DataMethodBodyBuilders.FormatKeyAsCSharpLiteral("test", null);

        Assert.That(result, Is.EqualTo("\"test\""));
    }

    [Test]
    public void FormatKeyAsCSharpLiteral_Int_ReturnsToString()
    {
        string result = DataMethodBodyBuilders.FormatKeyAsCSharpLiteral(42, null);

        Assert.That(result, Is.EqualTo("42"));
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return compilation;
    }

    private static ImmutableArray<MetadataReference> GetMetadataReferences()
    {
        string dotnetDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        List<MetadataReference> references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "netstandard.dll")),
            MetadataReference.CreateFromFile(typeof(Generate).Assembly.Location)
        };

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.IsNullOrEmpty(assembly.Location) && references.All(reference => reference.Display != assembly.Location))
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                catch (FileNotFoundException) { }
                catch (BadImageFormatException) { }
            }
        }

        return references.ToImmutableArray();
    }

    private static IMethodSymbol GetMethodSymbol(Compilation compilation, string typeName, string methodName)
    {
        INamedTypeSymbol? typeSymbol = compilation.GetTypeByMetadataName(typeName);
        Assert.That(typeSymbol, Is.Not.Null, $"Type '{typeName}' not found.");
        IMethodSymbol? methodSymbol = typeSymbol!.GetMembers(methodName).OfType<IMethodSymbol>().FirstOrDefault();
        Assert.That(methodSymbol, Is.Not.Null, $"Method '{methodName}' not found in '{typeName}'.");
        return methodSymbol!;
    }
}
