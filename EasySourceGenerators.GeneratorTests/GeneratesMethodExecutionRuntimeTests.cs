using System.Collections.Immutable;
using System.Reflection;
using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators;
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
                                                             public static IMethodImplementationGenerator Generate()
                                                             {
                                                                 return GenerateMethod();
                                                             }

                                                             private static IMethodImplementationGenerator GenerateMethod()
                                                             {
                                                                 return global::EasySourceGenerators.Abstractions.Generate.Method()
                                                                     .WithParameter<int>()
                                                                     .WithReturnType<int>()
                                                                     .WithSwitchBody()
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

    [Test]
    public void ExecuteFluentGeneratorMethod_ReturnsErrorWhenAbstractionsReferenceIsMissing()
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

        (SwitchBodyData? record, string? error) result =
            GeneratesMethodExecutionRuntime.ExecuteFluentGeneratorMethod(generatorMethod, partialMethod, compilation);

        Assert.That(result.record, Is.Null);
        Assert.That(result.error, Does.StartWith("Compilation failed:"));
    }

    private static CSharpCompilation CreateCompilation(string source, string assemblyName = "TestAssembly")
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
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
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDirectory, "System.Linq.dll")),
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
