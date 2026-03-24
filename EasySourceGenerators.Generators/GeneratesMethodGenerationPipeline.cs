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

        // NOTE: Explicit [SwitchCase] attribute-based generation is commented out and will be
        // replaced with a new approach in a future PR. The data abstraction layer will be
        // extended to support the new pattern. For now, only fluent and simple patterns are
        // supported through the data layer.
        if (hasSwitchCase || hasSwitchDefault)
        {
            return GeneratesMethodPatternSourceBuilder.GenerateFromSwitchAttributes(
                context,
                methods,
                firstMethod.PartialMethod,
                firstMethod.ContainingType,
                allPartials,
                compilation);
        }

        if (isFluentPattern)
        {
            return GenerateFromFluentPattern(context, methods[0], firstMethod, compilation);
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

    private static string GenerateFromFluentPattern(
        SourceProductionContext context,
        GeneratesMethodGenerationTarget methodInfo,
        GeneratesMethodGenerationTarget firstMethod,
        Compilation compilation)
    {
        (SwitchBodyData? record, string? error) = GeneratesMethodExecutionRuntime.ExecuteFluentGeneratorMethod(
            methodInfo.Symbol,
            firstMethod.PartialMethod,
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

        SwitchBodyData switchBodyData = record!;
        string? defaultExpression = switchBodyData.HasDefaultCase
            ? GeneratesMethodPatternSourceBuilder.ExtractDefaultExpressionFromFluentMethod(methodInfo.Syntax)
            : null;

        DataSwitchBody data = DataGeneratorsFactory.CreateSwitchBodyFromFluentData(switchBodyData, defaultExpression);
        return DataMethodBodyBuilders.BuildMethodSource(data, firstMethod.PartialMethod, firstMethod.ContainingType);
    }

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

        DataSimpleReturnBody data = DataGeneratorsFactory.CreateSimpleReturnBody(returnValue);
        return DataMethodBodyBuilders.BuildMethodSource(data, firstMethod.PartialMethod, firstMethod.ContainingType);
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
