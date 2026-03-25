using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static EasySourceGenerators.Generators.Consts;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Orchestrates the full source generation pipeline: collecting generation targets,
/// grouping them by target method, and generating C# source for each group.
/// </summary>
internal static class GeneratesMethodGenerationPipeline
{
    /// <summary>
    /// Executes the generation pipeline for all methods marked with the generator attribute.
    /// Collects valid targets, groups by containing type and target method, then generates source.
    /// </summary>
    internal static void Execute(
        SourceProductionContext context,
        ImmutableArray<MethodDeclarationSyntax?> generatorMethods,
        Compilation compilation)
    {
        List<GeneratesMethodGenerationTarget> validMethods = GeneratesMethodGenerationTargetCollector.Collect(context, generatorMethods, compilation);
        IReadOnlyList<IMethodSymbol> allPartials = GeneratesMethodExecutionRuntime.GetAllUnimplementedPartialMethods(compilation);

        IEnumerable<IGrouping<(string TypeKey, string TargetMethodName), GeneratesMethodGenerationTarget>> groups = validMethods
            .GroupBy(method => (TypeKey: method.ContainingType.ToDisplayString(), method.TargetMethodName));

        foreach (IGrouping<(string TypeKey, string TargetMethodName), GeneratesMethodGenerationTarget> group in groups)
        {
            List<GeneratesMethodGenerationTarget> methods = group.ToList();
            GeneratesMethodGenerationTarget firstMethod = methods[0];

            context.ReportDiagnostic(Diagnostic.Create(
                GeneratesMethodGeneratorDiagnostics.GeneratingMethodInfo,
                firstMethod.Syntax.GetLocation(),
                firstMethod.TargetMethodName,
                firstMethod.ContainingType.Name,
                string.Join(", ", methods.Select(method => method.Symbol.Name))));

            string source = GenerateSourceForGroup(context, methods, firstMethod, allPartials, compilation);

            if (!string.IsNullOrEmpty(source))
            {
                context.AddSource($"{firstMethod.ContainingType.Name}_{firstMethod.TargetMethodName}.g.cs", source);
            }
        }
    }

    /// <summary>
    /// Determines the generation pattern (fluent or simple) and generates source for a group
    /// of generator methods targeting the same partial method.
    /// </summary>
    private static string GenerateSourceForGroup(
        SourceProductionContext context,
        List<GeneratesMethodGenerationTarget> methods,
        GeneratesMethodGenerationTarget firstMethod,
        IReadOnlyList<IMethodSymbol> allPartials,
        Compilation compilation)
    {
        // SwitchCase attribute-based generation is commented out pending replacement with a data-driven approach.
        // See DataMethodBodyBuilders.cs for details on the planned replacement.
        // bool hasSwitchCase = methods.Any(method => HasAttribute(method.Symbol, SwitchCaseAttributeFullName));
        // bool hasSwitchDefault = methods.Any(method => HasAttribute(method.Symbol, SwitchDefaultAttributeFullName));
        bool isFluentPattern = methods.Count == 1 && methods[0].Symbol.ReturnType.ToDisplayString() == IMethodImplementationGeneratorFullName;

        if (isFluentPattern)
        {
            return GenerateFromFluentBodyPattern(
                context,
                methods[0],
                firstMethod.PartialMethod,
                firstMethod.ContainingType,
                compilation);
        }

        List<GeneratesMethodGenerationTarget> methodsWithParameters = methods
            .Where(method => method.Symbol.Parameters.Length > 0)
            .ToList();
        if (methodsWithParameters.Count > 0)
        {
            foreach (GeneratesMethodGenerationTarget methodWithParameters in methodsWithParameters)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratesMethodGeneratorDiagnostics.CannotUseRuntimeParameterForCompileTimeGeneratorError,
                    GetMethodSignatureLocation(methodWithParameters.Syntax)));
            }

            return string.Empty;
        }

        return GenerateFromSimplePattern(context, firstMethod, compilation);
    }

    /// <summary>
    /// Generates source code from a fluent body pattern. Executes the generator method first
    /// to obtain <see cref="FluentBodyResult"/>. If the result indicates a delegate body was
    /// provided (via <see cref="FluentBodyResult.HasDelegateBody"/>), attempts to extract the
    /// lambda body from the syntax tree. Otherwise, uses the runtime-evaluated return value.
    /// </summary>
    private static string GenerateFromFluentBodyPattern(
        SourceProductionContext context,
        GeneratesMethodGenerationTarget methodInfo,
        IMethodSymbol partialMethod,
        INamedTypeSymbol containingType,
        Compilation compilation)
    {
        (FluentBodyResult? result, string? error) = GeneratesMethodExecutionRuntime.ExecuteFluentBodyGeneratorMethod(
            methodInfo.Symbol,
            partialMethod,
            compilation);

        if (error != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GeneratesMethodGeneratorDiagnostics.GeneratorMethodExecutionError,
                methodInfo.Syntax.GetLocation(),
                methodInfo.Symbol.Name,
                error));
            return string.Empty;
        }

        if (result!.HasDelegateBody)
        {
            string? delegateBody = DelegateBodySyntaxExtractor.TryExtractDelegateBody(methodInfo.Syntax);
            if (delegateBody != null)
            {
                bool isVoidReturn = partialMethod.ReturnType.SpecialType == SpecialType.System_Void;
                string bodyLines = FormatDelegateBodyForEmit(delegateBody, isVoidReturn);

                return GeneratesMethodPatternSourceBuilder.GeneratePartialMethodWithBody(
                    containingType,
                    partialMethod,
                    bodyLines);
            }
        }

        return GeneratesMethodPatternSourceBuilder.GenerateSimplePartialMethod(
            containingType,
            partialMethod,
            result.ReturnValue);
    }

    /// <summary>
    /// Formats the extracted delegate body for emission. Expression bodies are wrapped in
    /// <c>return {expr};</c>. Block bodies are used as-is (already re-indented by the extractor).
    /// </summary>
    private static string FormatDelegateBodyForEmit(string delegateBody, bool isVoidReturn)
    {
        bool isBlockBody = delegateBody.Contains("\n");

        if (isBlockBody)
        {
            return delegateBody;
        }

        if (isVoidReturn)
        {
            return $"        {delegateBody};";
        }

        return $"        return {delegateBody};";
    }

    /// <summary>
    /// Generates source code from a simple pattern, executing the generator method
    /// and using its return value as the partial method's return expression.
    /// </summary>
    private static string GenerateFromSimplePattern(
        SourceProductionContext context,
        GeneratesMethodGenerationTarget firstMethod,
        Compilation compilation)
    {
        (string? returnValue, string? error) = GeneratesMethodExecutionRuntime.ExecuteSimpleGeneratorMethod(
            firstMethod.Symbol,
            firstMethod.PartialMethod,
            compilation);

        if (error != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GeneratesMethodGeneratorDiagnostics.GeneratorMethodExecutionError,
                firstMethod.Syntax.GetLocation(),
                firstMethod.Symbol.Name,
                error));
            return string.Empty;
        }

        return GeneratesMethodPatternSourceBuilder.GenerateSimplePartialMethod(
            firstMethod.ContainingType,
            firstMethod.PartialMethod,
            returnValue);
    }

    private static bool HasAttribute(IMethodSymbol methodSymbol, string fullAttributeTypeName)
    {
        return methodSymbol.GetAttributes()
            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == fullAttributeTypeName);
    }

    private static Location GetMethodSignatureLocation(MethodDeclarationSyntax methodSyntax)
    {
        TextSpan signatureSpan = TextSpan.FromBounds(methodSyntax.SpanStart, methodSyntax.ParameterList.Span.End);
        return Location.Create(methodSyntax.SyntaxTree, signatureSpan);
    }
}
