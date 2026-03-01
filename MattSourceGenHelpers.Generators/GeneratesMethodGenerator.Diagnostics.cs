using Microsoft.CodeAnalysis;

namespace MattSourceGenHelpers.Generators;

internal static class GeneratesMethodGeneratorDiagnostics
{
    private const string Category = "GeneratesMethodGenerator";

    internal static readonly DiagnosticDescriptor MissingPartialMethodError = new(
        id: "MSGH001",
        title: "Missing partial method",
        messageFormat: "Could not find partial method '{0}' in class '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor GeneratorMethodMustBeStaticError = new(
        id: "MSGH002",
        title: "Generator method must be static",
        messageFormat: "Method '{0}' marked with [GeneratesMethod] must be static",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor GeneratingMethodInfo = new(
        id: "MSGH003",
        title: "Generating partial method implementation",
        messageFormat: "Generating implementation for partial method '{0}' in class '{1}' using generator '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: false);

    internal static readonly DiagnosticDescriptor GeneratorMethodExecutionError = new(
        id: "MSGH004",
        title: "Generator method execution failed",
        messageFormat: "Failed to execute generator method '{0}': {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor GeneratorMethodTooManyParametersError = new(
        id: "MSGH005",
        title: "Generator method has too many parameters",
        messageFormat: "Method '{0}' marked with [GeneratesMethod] and [SwitchCase] has {1} parameter(s), but only methods with zero or one parameter are supported. Remove extra parameters or use the fluent API for more complex scenarios.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor SwitchCaseArgumentTypeMismatchError = new(
        id: "MSGH006",
        title: "SwitchCase argument type mismatch",
        messageFormat: "SwitchCase argument type '{0}' does not match the method parameter type '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
