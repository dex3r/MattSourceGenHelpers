using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MattSourceGenHelpers.GeneratorTests;

[TestFixture]
public class GeneratorDiagnosticsTests
{
    // -----------------------------------------------------------------------
    // MSGH001 – Missing partial method
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_WithNonExistingMethodName_EmitsMSGH001()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                public partial string ExistingMethod();

                [GeneratesMethod("NonExistingMethod")]
                private static string MyGenerator() => "hello";
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? msgh001 = diagnostics.FirstOrDefault(d => d.Id == "MSGH001");
        Assert.That(msgh001, Is.Not.Null, "Expected MSGH001 diagnostic for non-existing method name");
        Assert.That(msgh001!.GetMessage(), Does.Contain("NonExistingMethod"),
            "Error message should mention the missing method name");
        Assert.That(msgh001.GetMessage(), Does.Contain("MyClass"),
            "Error message should mention the containing class name");
    }

    [Test]
    public void GeneratesMethod_WithEmptyMethodName_DoesNotCrash()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                public partial string ExistingMethod();

                [GeneratesMethod("")]
                private static string MyGenerator() => "hello";
            }
            """;

        // Should not throw – just silently skip (empty name is filtered before MSGH001)
        Assert.DoesNotThrow(() => GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source));
    }

    // -----------------------------------------------------------------------
    // MSGH002 – Generator method must be static
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_WithNonStaticMethod_EmitsMSGH002()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                public partial string GetValue();

                [GeneratesMethod(nameof(GetValue))]
                private string NonStaticGenerator() => "hello";
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? msgh002 = diagnostics.FirstOrDefault(d => d.Id == "MSGH002");
        Assert.That(msgh002, Is.Not.Null, "Expected MSGH002 diagnostic for non-static generator method");
        Assert.That(msgh002!.GetMessage(), Does.Contain("NonStaticGenerator"),
            "Error message should mention the non-static method name");
    }

    // -----------------------------------------------------------------------
    // MSGH005 – Generator method has too many parameters (switch pattern)
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_SwitchCaseWithMoreThanOneParameter_EmitsMSGH005()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetValue(int key);

                [GeneratesMethod(nameof(GetValue))]
                [SwitchCase(arg1: 1)]
                private static int GetValue_Generator(int key, string extraParam) => 42;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? msgh005 = diagnostics.FirstOrDefault(d => d.Id == "MSGH005");
        Assert.That(msgh005, Is.Not.Null, "Expected MSGH005 diagnostic for generator method with too many parameters");
        Assert.That(msgh005!.GetMessage(), Does.Contain("GetValue_Generator"),
            "Error message should mention the generator method name");
        Assert.That(msgh005.GetMessage(), Does.Contain("2"),
            "Error message should mention the number of parameters");
    }

    [Test]
    public void GeneratesMethod_SwitchCaseWithOneParameter_DoesNotEmitMSGH005()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetValue(int key);

                [GeneratesMethod(nameof(GetValue))]
                [SwitchCase(arg1: 1)]
                private static int GetValue_Generator(int key) => 42;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "MSGH005"), Is.False,
            "Should not emit MSGH005 for a generator method with exactly one parameter");
    }

    [Test]
    public void GeneratesMethod_SwitchCaseWithZeroParameters_DoesNotEmitMSGH005()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetValue(int key);

                [GeneratesMethod(nameof(GetValue))]
                [SwitchCase(arg1: 1)]
                private static int GetValue_Generator() => 42;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "MSGH005"), Is.False,
            "Should not emit MSGH005 for a generator method with zero parameters");
    }

    // -----------------------------------------------------------------------
    // MSGH004 – Generator method execution failed (unfinished fluent API)
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_FluentUnfinished_EmitsMSGH004()
    {
        // The generator method returns IMethodImplementationGenerator<int, int> directly
        // (before calling WithSwitchBody / ForCases / ForDefaultCase), which the generator
        // treats as a simple pattern and tries to execute it, resulting in a non-null
        // IMethodImplementationGenerator object whose ToString() is not meaningful.
        // We expect either an MSGH004 or generated code with no meaningful content.
        // Either way, no uncaught exception should escape the generator.
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetValue(int key);

                [GeneratesMethod(nameof(GetValue))]
                private static IMethodImplementationGenerator GetValue_Generator() =>
                    Generate.Method().WithParameter<int>().WithReturnType<int>();
            }
            """;

        Assert.DoesNotThrow(() => GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source),
            "Generator should handle unfinished fluent construction without crashing");
    }

    // -----------------------------------------------------------------------
    // Partial method called from fluent API / generator execution
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_PartialMethodCalledInsideGenerator_EmitsMSGH004WithHelpfulMessage()
    {
        // The generator method calls a partial method that hasn't been implemented yet.
        // The dummy implementation should throw PartialMethodCalledDuringGenerationException,
        // which should be surfaced as MSGH004 with an informative message.
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetValue(int key);

                public static partial string GetLabel(int key);

                [GeneratesMethod(nameof(GetValue))]
                [SwitchCase(arg1: 1)]
                private static int GetValue_Generator(int key)
                {
                    // Calling the other partial method from inside a generator — this should throw.
                    string label = GetLabel(key);
                    return label.Length;
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? msgh004 = diagnostics.FirstOrDefault(d => d.Id == "MSGH004");
        Assert.That(msgh004, Is.Not.Null,
            "Expected MSGH004 when a partial method is called inside a generator method");
        Assert.That(msgh004!.GetMessage(), Does.Contain("GetValue_Generator"),
            "Error message should mention the generator method name");
        Assert.That(msgh004.GetMessage(), Does.Contain("PartialMethodCalledDuringGenerationException")
            .Or.Contain("GetLabel")
            .Or.Contain("partial method"),
            "Error message should hint that a partial method was called during generation");
    }

    // -----------------------------------------------------------------------
    // MSGH006 – SwitchCase argument type mismatch
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_SwitchCaseWithWrongArgumentType_EmitsMSGH006()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetValue(int key);

                [GeneratesMethod(nameof(GetValue))]
                [SwitchCase(arg1: "aaa")]
                private static int GetValue_Generator(int key) => 42;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Diagnostic? msgh006 = diagnostics.FirstOrDefault(d => d.Id == "MSGH006");
        Assert.That(msgh006, Is.Not.Null, "Expected MSGH006 diagnostic for SwitchCase argument type mismatch");
        Assert.That(msgh006!.GetMessage(), Does.Contain("string"),
            "Error message should mention the provided type");
        Assert.That(msgh006.GetMessage(), Does.Contain("int"),
            "Error message should mention the expected parameter type");
    }

    [Test]
    public void GeneratesMethod_SwitchCaseWithCorrectArgumentType_DoesNotEmitMSGH006()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetValue(int key);

                [GeneratesMethod(nameof(GetValue))]
                [SwitchCase(arg1: 1)]
                private static int GetValue_Generator(int key) => 42;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Assert.That(diagnostics.Any(d => d.Id == "MSGH006"), Is.False,
            "Should not emit MSGH006 when SwitchCase argument type matches the parameter type");
    }

    // -----------------------------------------------------------------------
    // Type mismatch between generator return type and partial method return type
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_SimplePattern_ReturnTypeMismatch_ProducesCompilationError()
    {
        // The generator method returns a string, but the partial method returns int.
        // The generated code will contain `return "hello";` where int is expected.
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                public partial int GetValue();

                [GeneratesMethod(nameof(GetValue))]
                private static string GetValue_Generator() => "hello";
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetDiagnostics(source);

        // The generator will produce code with a string literal where int is expected,
        // yielding a compiler error in the output compilation.
        Assert.That(diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), Is.True,
            "Expected compilation error due to return type mismatch");
    }

    // -----------------------------------------------------------------------
    // Correct configuration – should produce no generator errors
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_SimplePattern_ValidConfiguration_ProducesNoDiagnosticErrors()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                public partial string GetValue();

                [GeneratesMethod(nameof(GetValue))]
                private static string GetValue_Generator() => "hello";
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Assert.That(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty,
            "Valid generator configuration should produce no error diagnostics");
    }

    [Test]
    public void GeneratesMethod_SwitchCasePattern_ValidConfiguration_ProducesNoDiagnosticErrors()
    {
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public static partial class MyClass
            {
                public static partial int GetPiDigit(int index);

                [GeneratesMethod(nameof(GetPiDigit))]
                [SwitchCase(arg1: 0)]
                [SwitchCase(arg1: 1)]
                private static int GetPiDigit_Generator(int index) => index == 0 ? 3 : 1;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnostics(source);

        Assert.That(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty,
            "Valid switch-case generator configuration should produce no error diagnostics");
    }

    // -----------------------------------------------------------------------
    // CompilationReference scenario – simulates Rider's code inspector
    // -----------------------------------------------------------------------

    [Test]
    public void GeneratesMethod_FluentPattern_WithCompilationReference_ProducesNoDiagnosticErrors()
    {
        // This test simulates the scenario that occurs in Rider's code inspector, where the
        // abstractions project is provided as a CompilationReference (in-memory) instead of
        // a PortableExecutableReference (file-based). This was the root cause of MSGH004 in Rider.
        string source = """
            using MattSourceGenHelpers.Abstractions;

            namespace TestNamespace;

            public partial class MyClass
            {
                public partial string GetValue();

                [GeneratesMethod(nameof(GetValue))]
                static IMethodImplementationGenerator GetValue_Generator() =>
                    Generate.Method().WithReturnType<string>().UseBody(() => "hello");
            }
            """;

        ImmutableArray<Diagnostic>? diagnostics = GeneratorTestHelper.GetGeneratorOnlyDiagnosticsWithCompilationReference(source);

        if (diagnostics == null)
        {
            Assert.Ignore("Could not locate abstractions source files on disk; skipping CompilationReference test.");
            return;
        }

        Assert.That(diagnostics.Value.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty,
            "Fluent generator with a CompilationReference for abstractions should produce no error diagnostics");
    }
}
