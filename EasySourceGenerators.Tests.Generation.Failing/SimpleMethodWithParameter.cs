using System.Collections.Immutable;
using EasySourceGenerators.Tests.Generators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EasySourceGenerators.Tests.Generation.Failing;

public class SimpleMethodWithParameterTests
{
    [Test]
    public void SimpleMethodWithParameterTests_Test()
    {
        string source = """
                        using EasySourceGenerators.Abstractions;

                        namespace TestNamespace;

                        public partial class SimpleMethodWithParameterClass
                        {
                            public partial int SimpleMethodWithParameter(int someIntParameter);

                            [MethodBodyGenerator(sameClassMethodName: nameof(SimpleMethodWithParameter))]
                            private static int SimpleMethodWithParameter_Generator(int someIntParameter)
                            {
                                return 5;
                            }
                        }
                        """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? msgh007 = diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == "MSGH007");
        Assert.That(msgh007, Is.Not.Null, "Expected MSGH007 for simple generator method with runtime parameter.");
        Assert.That(msgh007!.Location.IsInSource, Is.True, "MSGH007 should point to generator source.");

        TextSpan span = msgh007.Location.SourceSpan;
        string highlightedCode = source.Substring(span.Start, span.Length);
        Assert.That(highlightedCode, Does.Contain("SimpleMethodWithParameter_Generator(int someIntParameter)"));
        Assert.That(highlightedCode, Does.Not.Contain("return 5;"),
            "MSGH007 should highlight the generator method signature, not the method body.");
    }
}
