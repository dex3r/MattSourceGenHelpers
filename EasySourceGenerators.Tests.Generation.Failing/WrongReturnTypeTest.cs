using System.Collections.Immutable;
using EasySourceGenerators.Tests.Generators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EasySourceGenerators.Tests.Generation.Failing;

public class WrongReturnType
{
    [Test]
    public void WrongReturnTypeTest_Object()
    {
        string source = """
                        using EasySourceGenerators.Abstractions;

                        namespace TestNamespace;

                        public static partial class JustReturnConstantTestClass
                        {
                            public static partial int JustReturnConstant();
                        
                            [MethodBodyGenerator(nameof(JustReturnConstant))]
                            public static object JustReturnConstantGenerator() =>
                                Generate.MethodBody()
                                    .ForMethod().WithReturnType<int>().WithNoParameters()
                                    .UseProvidedBody(() => 42);
                        }
                        """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? diag = diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == "MSGH008");
        Assert.That(diag, Is.Not.Null);
        Assert.That(diag!.Location.IsInSource, Is.True);

        TextSpan span = diag.Location.SourceSpan;
        string highlightedCode = source.Substring(span.Start, span.Length);
        Assert.That(highlightedCode, Is.EqualTo("object"), "Diagnostic should highlight only the return type");
    }
    
    [Test]
    public void WrongReturnTypeTest_Double()
    {
        string source = """
                        using EasySourceGenerators.Abstractions;

                        namespace TestNamespace;

                        public static partial class JustReturnConstantTestClass
                        {
                            public static partial int JustReturnConstant();

                            [MethodBodyGenerator(nameof(JustReturnConstant))]
                            public static double JustReturnConstantGenerator() => 42.0;
                        }
                        """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? diag = diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == "MSGH008");
        Assert.That(diag, Is.Not.Null);
        Assert.That(diag!.Location.IsInSource, Is.True);

        TextSpan span = diag.Location.SourceSpan;
        string highlightedCode = source.Substring(span.Start, span.Length);
        Assert.That(highlightedCode, Is.EqualTo("double"), "Diagnostic should highlight only the return type");
    }
    
    [Test]
    public void WrongReturnTypeTest_String()
    {
        string source = """
                        using EasySourceGenerators.Abstractions;

                        namespace TestNamespace;

                        public static partial class JustReturnConstantTestClass
                        {
                            public static partial int JustReturnConstant();

                            [MethodBodyGenerator(nameof(JustReturnConstant))]
                            public static string JustReturnConstantGenerator() => "abc";
                        }
                        """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? diag = diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == "MSGH008");
        Assert.That(diag, Is.Not.Null);
        Assert.That(diag!.Location.IsInSource, Is.True);

        TextSpan span = diag.Location.SourceSpan;
        string highlightedCode = source.Substring(span.Start, span.Length);
        Assert.That(highlightedCode, Is.EqualTo("string"), "Diagnostic should highlight only the return type");
    }
}