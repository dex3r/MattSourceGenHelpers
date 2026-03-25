using System.Collections.Immutable;
using System.Reflection;
using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators;
using EasySourceGenerators.Generators.OldGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class GeneratesMethodExecutionRuntimeTests
{
    [Test]
    public void GetAllUnimplementedPartialMethods_ReturnsOnlyPartialDefinitions()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public partial class Sample
                                                         {
                                                             public partial string NeedsImplementation();
                                                             public partial string AlreadyImplemented() => "done";
                                                             public string NonPartial() => "x";
                                                         }
                                                         """);

        IReadOnlyList<IMethodSymbol> methods = GeneratesMethodExecutionRuntime.GetAllUnimplementedPartialMethods(compilation);

        Assert.That(methods.Count, Is.EqualTo(1));
        Assert.That(methods[0].Name, Is.EqualTo("NeedsImplementation"));
    }

    [Test]
    public void ExecuteSimpleGeneratorMethod_ExecutesStaticMethodWithoutArguments()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public partial class Target
                                                         {
                                                             public partial string GetValue();
                                                         }

                                                         public static class GenHost
                                                         {
                                                             public static string Generate() => "hello";
                                                         }
                                                         """);

        IMethodSymbol generatorMethod = GetMethodSymbol(compilation, "TestNamespace.GenHost", "Generate");
        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.Target", "GetValue");

        (string? value, string? error) result =
            GeneratesMethodExecutionRuntime.ExecuteSimpleGeneratorMethod(generatorMethod, partialMethod, compilation);

        Assert.That(result.error, Is.Null);
        Assert.That(result.value, Is.EqualTo("hello"));
    }

    [Test]
    public void ExecuteSimpleGeneratorMethod_ExecutesWhenCompilationHasTopLevelStatements()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         using System;

                                                         Console.WriteLine("warmup");

                                                         namespace TestNamespace
                                                         {
                                                             public partial class Target
                                                             {
                                                                 public partial string GetValue();
                                                             }

                                                             public static class GenHost
                                                             {
                                                                 public static string Generate() => "hello";
                                                             }
                                                         }
                                                         """,
            outputKind: OutputKind.ConsoleApplication);

        IMethodSymbol generatorMethod = GetMethodSymbol(compilation, "TestNamespace.GenHost", "Generate");
        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.Target", "GetValue");

        (string? value, string? error) result =
            GeneratesMethodExecutionRuntime.ExecuteSimpleGeneratorMethod(generatorMethod, partialMethod, compilation);

        Assert.That(result.error, Is.Null);
        Assert.That(result.value, Is.EqualTo("hello"));
    }

    [Test]
    public void ExecuteGeneratorMethodWithArgs_ConvertsArgumentsToMethodParameterType()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static class GenHost
                                                         {
                                                             public static string Generate(int value) => (value + 1).ToString();
                                                         }
                                                         """);

        IMethodSymbol generatorMethod = GetMethodSymbol(compilation, "TestNamespace.GenHost", "Generate");

        (string? value, string? error) result = GeneratesMethodExecutionRuntime.ExecuteGeneratorMethodWithArgs(
            generatorMethod,
            Array.Empty<IMethodSymbol>(),
            compilation,
            new object?[] { "41" });

        Assert.That(result.error, Is.Null);
        Assert.That(result.value, Is.EqualTo("42"));
    }

    [Test]
    public void ExecuteGeneratorMethodWithArgs_ReturnsErrorWhenTypeCannotBeFound()
    {
        CSharpCompilation symbolCompilation = CreateCompilation("""
                                                               namespace TestNamespace;

                                                               public static class GenHost
                                                               {
                                                                   public static string Generate() => "hello";
                                                               }
                                                               """);
        CSharpCompilation executionCompilation = CreateCompilation("""
                                                                  namespace AnotherNamespace;

                                                                  public static class DifferentType
                                                                  {
                                                                      public static string Generate() => "hello";
                                                                  }
                                                                  """);

        IMethodSymbol generatorMethod = GetMethodSymbol(symbolCompilation, "TestNamespace.GenHost", "Generate");

        (string? value, string? error) result = GeneratesMethodExecutionRuntime.ExecuteGeneratorMethodWithArgs(
            generatorMethod,
            Array.Empty<IMethodSymbol>(),
            executionCompilation,
            null);

        Assert.That(result.value, Is.Null);
        Assert.That(result.error, Does.Contain("Could not find type 'TestNamespace.GenHost'"));
    }

    [Test]
    public void ExecuteGeneratorMethodWithArgs_ReturnsErrorWhenGeneratorThrowsException()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         using System;

                                                         namespace TestNamespace;

                                                         public static class GenHost
                                                         {
                                                             public static string Generate()
                                                             {
                                                                 throw new InvalidOperationException("boom");
                                                             }
                                                         }
                                                         """);

        IMethodSymbol generatorMethod = GetMethodSymbol(compilation, "TestNamespace.GenHost", "Generate");

        (string? value, string? error) result = GeneratesMethodExecutionRuntime.ExecuteGeneratorMethodWithArgs(
            generatorMethod,
            Array.Empty<IMethodSymbol>(),
            compilation,
            null);

        Assert.That(result.value, Is.Null);
        Assert.That(result.error, Does.Contain("InvalidOperationException"));
        Assert.That(result.error, Does.Contain("boom"));
    }

    [Test]
    public void ExecuteGeneratorMethodWithArgs_ReturnsErrorWhenArgumentConversionFails()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         namespace TestNamespace;

                                                         public static class GenHost
                                                         {
                                                             public static string Generate(int value) => value.ToString();
                                                         }
                                                         """);

        IMethodSymbol generatorMethod = GetMethodSymbol(compilation, "TestNamespace.GenHost", "Generate");

        (string? value, string? error) result = GeneratesMethodExecutionRuntime.ExecuteGeneratorMethodWithArgs(
            generatorMethod,
            Array.Empty<IMethodSymbol>(),
            compilation,
            new object?[] { "not-an-int" });

        Assert.That(result.value, Is.Null);
        Assert.That(result.error, Does.Contain("FormatException"));
    }

    // ExecuteFluentGeneratorMethod with SwitchBodyData is commented out pending replacement
    // with the data abstraction layer. See DataMethodBodyBuilders.cs for details.
    /*
    [Test]
    public void ExecuteFluentGeneratorMethod_CollectsSwitchBodyData()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         using EasySourceGenerators.Abstractions;

                                                         namespace TestNamespace;

                                                         public static partial class Target
                                                         {
                                                             public static partial int GetValue(int input);
                                                         }

                                                         public static class GenHost
                                                         {
                                                             public static IMethodBodyGenerator Generate()
                                                             {
                                                                 return GenerateMethod();
                                                             }

                                                             private static IMethodBodyGenerator GenerateMethod()
                                                             {
                                                                 return global::EasySourceGenerators.Abstractions.Generate.Method()
                                                                     .WithOneParameter<int>()
                                                                     .WithReturnType<int>()
                                                                     .GenerateSwitchBody()
                                                                     .ForCases(1, new[] { 2, 3 }, "4")
                                                                     .ReturnConstantValue(value => value * 2)
                                                                     .ForDefaultCase()
                                                                     .ReturnConstantValue(_ => 0);
                                                             }
                                                         }
                                                         """);

        IMethodSymbol generatorMethod = GetMethodSymbol(compilation, "TestNamespace.GenHost", "Generate");
        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.Target", "GetValue");

        (SwitchBodyData? record, string? error) result =
            GeneratesMethodExecutionRuntime.ExecuteFluentGeneratorMethod(generatorMethod, partialMethod, compilation);

        Assert.That(result.error, Is.Null);
        Assert.That(result.record, Is.Not.Null);
        Assert.That(result.record!.HasDefaultCase, Is.True);
        Assert.That(result.record.CasePairs.Select(pair => pair.key), Is.EqualTo(new object[] { 1, 2, 3, 4 }));
        Assert.That(result.record.CasePairs.Select(pair => pair.value), Is.EqualTo(new[] { "2", "4", "6", "8" }));
    }
    */

    [Test]
    public void ExecuteFluentBodyGeneratorMethod_WithBodyReturningConstant_ExtractsReturnValue()
    {
        CSharpCompilation compilation = CreateCompilation("""
                                                         using EasySourceGenerators.Abstractions;

                                                         namespace TestNamespace;

                                                         public static partial class Target
                                                         {
                                                             public static partial string GetValue();
                                                         }

                                                         public static class GenHost
                                                         {
                                                             public static IMethodBodyGenerator Generate()
                                                             {
                                                                 return global::EasySourceGenerators.Abstractions.Generate
                                                                     .MethodBody()
                                                                     .ForMethod()
                                                                     .WithReturnType<string>()
                                                                     .WithNoParameters()
                                                                     .BodyReturningConstant(() => "hello fluent");
                                                             }
                                                         }
                                                         """);

        IMethodSymbol generatorMethod = GetMethodSymbol(compilation, "TestNamespace.GenHost", "Generate");
        IMethodSymbol partialMethod = GetMethodSymbol(compilation, "TestNamespace.Target", "GetValue");

        (FluentBodyResult? result, string? error) outcome =
            GeneratesMethodExecutionRuntime.ExecuteFluentBodyGeneratorMethod(generatorMethod, partialMethod, compilation);

        Assert.That(outcome.error, Is.Null);
        Assert.That(outcome.result, Is.Not.Null);
        Assert.That(outcome.result!.ReturnValue, Is.EqualTo("hello fluent"));
        Assert.That(outcome.result.IsVoid, Is.False);
    }

    [Test]
    public void ExecuteFluentBodyGeneratorMethod_ReturnsErrorWhenAbstractionsReferenceIsMissing()
    {
        CSharpCompilation originalCompilation = CreateCompilation("""
                                                                 namespace TestNamespace;

                                                                 public static partial class Target
                                                                 {
                                                                     public static partial int GetValue(int input);
                                                                 }

                                                                 public static class GenHost
                                                                 {
                                                                     public static string Generate()
                                                                     {
                                                                         return "x";
                                                                     }
                                                                 }
                                                                 """);

        IEnumerable<MetadataReference> referencesWithoutAbstractions = originalCompilation.References
            .Where(reference => reference.Display == null
                                || !reference.Display.Contains(Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase));
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "MissingAbstractionsTest",
            syntaxTrees: originalCompilation.SyntaxTrees,
            references: referencesWithoutAbstractions,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        IMethodSymbol generatorMethod = GetMethodSymbol(originalCompilation, "TestNamespace.GenHost", "Generate");
        IMethodSymbol partialMethod = GetMethodSymbol(originalCompilation, "TestNamespace.Target", "GetValue");

        (FluentBodyResult? result, string? error) outcome =
            GeneratesMethodExecutionRuntime.ExecuteFluentBodyGeneratorMethod(generatorMethod, partialMethod, compilation);

        Assert.That(outcome.result, Is.Null);
        Assert.That(outcome.error, Does.StartWith("Compilation failed:"));
    }

    private static CSharpCompilation CreateCompilation(
        string source,
        string assemblyName = "TestAssembly",
        OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(outputKind));
        return compilation;
    }

    private static ImmutableArray<MetadataReference> GetMetadataReferences()
    {
        string dotnetDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        List<MetadataReference> references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "System.Linq.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "System.Console.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "netstandard.dll")),
            MetadataReference.CreateFromFile(typeof(Generate).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(GeneratesMethodExecutionRuntime).Assembly.Location)
        };

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.IsNullOrEmpty(assembly.Location) && references.All(reference => reference.Display != assembly.Location))
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                catch (FileNotFoundException)
                {
                    // AppDomain can contain dynamic/transient entries where Location points to a file that
                    // is no longer available. These references are optional for this test helper, so we skip them.
                }
                catch (BadImageFormatException)
                {
                    // Some loaded modules are native or otherwise not valid managed PE metadata references.
                    // They cannot be used by Roslyn, and skipping them is safe because core references are added above.
                }
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
