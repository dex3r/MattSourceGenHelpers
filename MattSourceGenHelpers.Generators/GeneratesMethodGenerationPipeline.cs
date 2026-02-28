using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using static MattSourceGenHelpers.Generators.Consts;

namespace MattSourceGenHelpers.Generators;

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
            return GeneratesMethodPatternSourceBuilder.GenerateFromFluent(
                context,
                methods[0],
                firstMethod.PartialMethod,
                firstMethod.ContainingType,
                compilation);
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
}
