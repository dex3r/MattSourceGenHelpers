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

    /// <summary>
    /// Returns only the diagnostics emitted by the generator itself, using a
    /// <see cref="CompilationReference"/> for the abstractions assembly instead of a
    /// <see cref="PortableExecutableReference"/>. This simulates how Rider's code inspector
    /// provides in-memory project references rather than file-based references.
    /// Returns <c>null</c> if the abstractions source files cannot be located on disk.
    /// </summary>
    internal static ImmutableArray<Diagnostic>? GetGeneratorOnlyDiagnosticsWithCompilationReference(string source)
    {
        CompilationReference? abstractionsRef = CreateAbstractionsCompilationReference();
        if (abstractionsRef == null)
            return null;

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Use standard references but replace the abstractions PE reference with the CompilationReference.
        List<MetadataReference> references = GetMetadataReferences()
            .Where(r => r.Display?.Contains("MattSourceGenHelpers.Abstractions", StringComparison.OrdinalIgnoreCase) != true)
            .ToList();
        references.Add(abstractionsRef);

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

    /// <summary>
    /// Compiles the abstractions project source files into an in-memory
    /// <see cref="CompilationReference"/>, simulating how an IDE provides project references.
    /// Returns <c>null</c> if the source files cannot be found on disk or compilation fails.
    /// </summary>
    private static CompilationReference? CreateAbstractionsCompilationReference()
    {
        string? abstractionsSrcDir = FindAbstractionsSourceDirectory();
        if (abstractionsSrcDir == null)
            return null;

        string[] sourceFiles = Directory.GetFiles(abstractionsSrcDir, "*.cs", SearchOption.AllDirectories);
        if (sourceFiles.Length == 0)
            return null;

        CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
        List<SyntaxTree> syntaxTrees = sourceFiles
            .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file), parseOptions))
            .ToList();

        // Collect metadata references: start with already-loaded assemblies, then also add any
        // DLLs in the test binary directory to pick up JetBrains.Annotations and similar packages
        // that may not have been loaded into the AppDomain yet.
        HashSet<string> addedPaths = new(StringComparer.OrdinalIgnoreCase);
        List<MetadataReference> references = new();

        void TryAddFile(string path)
        {
            if (!addedPaths.Add(path))
                return;
            try { references.Add(MetadataReference.CreateFromFile(path)); }
            catch (Exception) { /* skip unreadable assemblies */ }
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.IsNullOrEmpty(assembly.Location))
                TryAddFile(assembly.Location);
        }

        // Also scan the test binary directory for DLLs not yet loaded (e.g., JetBrains.Annotations).
        string testBinDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        foreach (string dll in Directory.GetFiles(testBinDir, "*.dll"))
            TryAddFile(dll);

        CSharpCompilation abstractionsCompilation = CSharpCompilation.Create(
            assemblyName: "MattSourceGenHelpers.Abstractions",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        if (abstractionsCompilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
            return null;

        return (CompilationReference)abstractionsCompilation.ToMetadataReference();
    }

    private static string? FindAbstractionsSourceDirectory()
    {
        string? dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        for (int i = 0; i < 10; i++)
        {
            if (dir == null)
                return null;
            string candidate = Path.Combine(dir, "MattSourceGenHelpers.Abstractions");
            if (Directory.Exists(candidate) && Directory.GetFiles(candidate, "*.cs").Length > 0)
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }

        return null;
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
