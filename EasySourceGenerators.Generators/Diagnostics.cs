using EasySourceGenerators.Abstractions;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators;

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
        messageFormat: $"Method '{{0}}' marked with [{nameof(MethodBodyGenerator)}] must be static",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor GeneratingMethodInfo = new(
        id: "MSGH003",
        title: "Generating partial method implementation",
        messageFormat: "Generating implementation for partial method '{0}' in class '{1}' using generator '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor GeneratorMethodExecutionError = new(
        id: "MSGH004",
        title: "Generator method execution failed",
        messageFormat: "Failed to execute generator method '{0}': {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    internal static readonly DiagnosticDescriptor CannotUseRuntimeParameterForCompileTimeGeneratorError = new(
        id: "MSGH007",
        title: "Cannot use runtime parameter for compile-time generator",
        messageFormat: "Method generators cannot have any parameters, as they will be run at compile time to generate the method output value. Use MethodTemplate if you want to emit method body instead of single value.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    internal static readonly DiagnosticDescriptor MethodBodyGeneratorInvalidReturnType = new(
        id: "MSGH008",
        title: "MethodBodyGenerator has invalid return type",
        messageFormat: $"Method '{{0}}' marked with [{nameof(MethodBodyGenerator)}] must return either IMethodBodyGenerator (for fluent API builder) or the exact type the target method returns (for compile type const return body)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
