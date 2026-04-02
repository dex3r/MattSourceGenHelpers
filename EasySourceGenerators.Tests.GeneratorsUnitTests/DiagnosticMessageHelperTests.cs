using EasySourceGenerators.Generators.IncrementalGenerators;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Tests.Generators;

[TestFixture]
public class DiagnosticMessageHelperTests
{
    [Test]
    public void JoinErrorDiagnostics_EmptyList_ReturnsEmptyString()
    {
        string result = DiagnosticMessageHelper.JoinErrorDiagnostics(Array.Empty<Diagnostic>());

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void JoinErrorDiagnostics_OnlyWarnings_ReturnsEmptyString()
    {
        Diagnostic[] diagnostics = new[]
        {
            CreateDiagnostic(DiagnosticSeverity.Warning, "This is a warning")
        };

        string result = DiagnosticMessageHelper.JoinErrorDiagnostics(diagnostics);

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void JoinErrorDiagnostics_SingleError_ReturnsSingleMessage()
    {
        Diagnostic[] diagnostics = new[]
        {
            CreateDiagnostic(DiagnosticSeverity.Error, "Something went wrong")
        };

        string result = DiagnosticMessageHelper.JoinErrorDiagnostics(diagnostics);

        Assert.That(result, Is.EqualTo("Something went wrong"));
    }

    [Test]
    public void JoinErrorDiagnostics_MultipleErrors_JoinsWithSemicolon()
    {
        Diagnostic[] diagnostics = new[]
        {
            CreateDiagnostic(DiagnosticSeverity.Error, "First error"),
            CreateDiagnostic(DiagnosticSeverity.Error, "Second error")
        };

        string result = DiagnosticMessageHelper.JoinErrorDiagnostics(diagnostics);

        Assert.That(result, Is.EqualTo("First error; Second error"));
    }

    [Test]
    public void JoinErrorDiagnostics_MixedSeverities_ReturnsOnlyErrors()
    {
        Diagnostic[] diagnostics = new[]
        {
            CreateDiagnostic(DiagnosticSeverity.Warning, "A warning"),
            CreateDiagnostic(DiagnosticSeverity.Error, "An error"),
            CreateDiagnostic(DiagnosticSeverity.Info, "An info")
        };

        string result = DiagnosticMessageHelper.JoinErrorDiagnostics(diagnostics);

        Assert.That(result, Is.EqualTo("An error"));
    }

    private static Diagnostic CreateDiagnostic(DiagnosticSeverity severity, string message)
    {
        DiagnosticDescriptor descriptor = new(
            id: "TEST001",
            title: "Test",
            messageFormat: message,
            category: "Test",
            defaultSeverity: severity,
            isEnabledByDefault: true);

        return Diagnostic.Create(descriptor, Location.None);
    }
}
