using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using static EasySourceGenerators.Generators.Consts;

namespace EasySourceGenerators.Generators;

internal static class GeneratesMethodGenerationPipeline
{
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

    private static string GenerateSourceForGroup(
        SourceProductionContext context,
        List<GeneratesMethodGenerationTarget> methods,
        GeneratesMethodGenerationTarget firstMethod,
        IReadOnlyList<IMethodSymbol> allPartials,
        Compilation compilation)
    {
        bool hasSwitchCase = methods.Any(method => HasAttribute(method.Symbol, SwitchCaseAttributeFullName));
        bool hasSwitchDefault = methods.Any(method => HasAttribute(method.Symbol, SwitchDefaultAttributeFullName));
        bool isFluentPattern = methods.Count == 1 && methods[0].Symbol.ReturnType.ToDisplayString() == IMethodImplementationGeneratorFullName;
        bool isEntireMethodGeneration = firstMethod.PartialMethod is null;

        if (hasSwitchCase || hasSwitchDefault)
        {
            return GeneratesMethodPatternSourceBuilder.GenerateFromSwitchAttributes(
                context,
                methods,
                firstMethod.PartialMethod!,
                firstMethod.ContainingType,
                allPartials,
                compilation);
        }

        if (isFluentPattern && isEntireMethodGeneration)
        {
            return GenerateFromEntireMethodPattern(context, methods[0], compilation);
        }

        if (isFluentPattern)
        {
            return GeneratesMethodPatternSourceBuilder.GenerateFromFluent(
                context,
                methods[0],
                firstMethod.PartialMethod!,
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

    private static string GenerateFromSimplePattern(
        SourceProductionContext context,
        GeneratesMethodGenerationTarget firstMethod,
        Compilation compilation)
    {
        (string? returnValue, string? error) = GeneratesMethodExecutionRuntime.ExecuteSimpleGeneratorMethod(
            firstMethod.Symbol,
            firstMethod.PartialMethod!,
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
            firstMethod.PartialMethod!,
            returnValue);
    }

    private static string GenerateFromEntireMethodPattern(
        SourceProductionContext context,
        GeneratesMethodGenerationTarget methodInfo,
        Compilation compilation)
    {
        (MethodData? methodData, string? error) = GeneratesMethodExecutionRuntime.ExecuteEntireMethodGeneratorMethod(
            methodInfo.Symbol,
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

        MethodData data = methodData!;

        if (data.SwitchBody != null)
        {
            string? defaultExpression = data.SwitchBody.HasDefaultCase
                ? GeneratesMethodPatternSourceBuilder.ExtractDefaultExpressionFromFluentMethod(methodInfo.Syntax)
                : null;

            return GeneratesMethodPatternSourceBuilder.GenerateEntireMethodWithSwitch(
                methodInfo.ContainingType,
                data,
                defaultExpression);
        }

        return GeneratesMethodPatternSourceBuilder.GenerateEntireMethodSimple(
            methodInfo.ContainingType,
            data);
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
