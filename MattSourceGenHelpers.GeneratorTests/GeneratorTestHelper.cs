using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MattSourceGenHelpers.Generators;

namespace MattSourceGenHelpers.GeneratorTests;

/// <summary>
/// Helper for running the <see cref="GeneratesMethodGenerator"/> against in-memory source code
/// and capturing its diagnostic output.
/// </summary>
internal static class GeneratorTestHelper
{
    /// <summary>
    /// Runs the source generator against the given source code and returns all diagnostics
    /// reported by the generator as well as any compilation errors in the generated output.
    /// </summary>
    internal static ImmutableArray<Diagnostic> GetDiagnostics(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<MetadataReference> references = GetMetadataReferences();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratesMethodGenerator generator = new();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> generatorDiagnostics);

        // Include both generator diagnostics and compilation errors from the output
        ImmutableArray<Diagnostic> compilationDiagnostics = outputCompilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        return generatorDiagnostics.AddRange(compilationDiagnostics);
    }

    /// <summary>
    /// Returns only the diagnostics emitted by the generator itself (not compilation errors).
    /// </summary>
    internal static ImmutableArray<Diagnostic> GetGeneratorOnlyDiagnostics(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<MetadataReference> references = GetMetadataReferences();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratesMethodGenerator generator = new();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out ImmutableArray<Diagnostic> generatorDiagnostics);

        return generatorDiagnostics;
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        // Core runtime references
        string dotnetDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        List<MetadataReference> references = new()
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDir, "System.Collections.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDir, "System.Linq.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotnetDir, "netstandard.dll")),
            // Abstractions assembly
            MetadataReference.CreateFromFile(typeof(MattSourceGenHelpers.Abstractions.GeneratesMethod).Assembly.Location),
        };

        // Add any other loaded assemblies that might be needed (e.g., System.Private.CoreLib)
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.IsNullOrEmpty(assembly.Location) &&
                !references.Any(reference => reference.Display == assembly.Location))
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                catch (FileNotFoundException)
                {
                    // Skip assemblies whose files are no longer on disk
                }
                catch (BadImageFormatException)
                {
                    // Skip assemblies with an unreadable PE format
                }
            }
        }

        return references;
    }
}
