using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class GeneratesMethodGenerationTargetCollectorTests
{
    [Test]
    public void GeneratesMethod_WhenGeneratorIsNonStaticAndTargetIsMissing_EmitsOnlyMSGH002()
    {
        string source = """
            using EasySourceGenerators.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                [MethodBodyGenerator("MissingPartial")]
                private string MyGenerator() => "hello";
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Assert.That(diagnostics.Count(diagnostic => diagnostic.Id == "MSGH002"), Is.EqualTo(1),
            "Collector should report non-static generator method first.");
        Assert.That(diagnostics.Any(diagnostic => diagnostic.Id == "MSGH001"), Is.False,
            "Collector should stop processing this method after MSGH002 and not emit missing partial diagnostics.");
    }

    [Test]
    public void GeneratesMethod_WithWhitespaceTargetName_EmitsMSGH001()
    {
        string source = """
            using EasySourceGenerators.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                public partial string ExistingMethod();

                [MethodBodyGenerator("   ")]
                private static string MyGenerator() => "hello";
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? msgh001 = diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == "MSGH001");
        Assert.That(msgh001, Is.Not.Null);
        Assert.That(msgh001!.GetMessage(), Does.Contain("MyClass"));
    }
}
