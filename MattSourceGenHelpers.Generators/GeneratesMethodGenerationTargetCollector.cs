using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MattSourceGenHelpers.Abstractions;
using System.Collections.Immutable;
using static MattSourceGenHelpers.Generators.Consts;

namespace MattSourceGenHelpers.Generators;

internal sealed record GeneratesMethodGenerationTarget(
    MethodDeclarationSyntax Syntax,
    IMethodSymbol Symbol,
    string TargetMethodName,
    IMethodSymbol PartialMethod,
    INamedTypeSymbol ContainingType);

internal static class GeneratesMethodGenerationTargetCollector
{
    internal static List<GeneratesMethodGenerationTarget> Collect(
        SourceProductionContext context,
        ImmutableArray<MethodDeclarationSyntax?> generatorMethods,
        Compilation compilation)
    {
        List<GeneratesMethodGenerationTarget> validMethods = new();

        foreach (MethodDeclarationSyntax? generatorMethod in generatorMethods)
        {
            if (generatorMethod is null)
            {
                continue;
            }

            SemanticModel semanticModel = compilation.GetSemanticModel(generatorMethod.SyntaxTree);
            IMethodSymbol? methodSymbol = semanticModel.GetDeclaredSymbol(generatorMethod) as IMethodSymbol;

            if (methodSymbol is null)
            {
                continue;
            }

            if (!methodSymbol.IsStatic)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratesMethodGeneratorDiagnostics.GeneratorMethodMustBeStaticError,
                    generatorMethod.GetLocation(),
                    generatorMethod.Identifier.Text));
                continue;
            }

            AttributeData? attribute = methodSymbol
                .GetAttributes()
                .FirstOrDefault(attributeData => attributeData.AttributeClass?.ToDisplayString() == GeneratesMethodAttributeFullName);

            if (attribute is null || attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            ImmutableArray<IParameterSymbol> constructorParameters = attribute.AttributeConstructor?.Parameters ?? [];
            int targetMethodNameArgumentIndex = constructorParameters
                .Select((parameter, index) => new { parameter, index })
                .Where(value => value.parameter.Name.Equals(nameof(GeneratesMethod.SameClassMethodName), StringComparison.OrdinalIgnoreCase))
                .Select(value => value.index)
                .DefaultIfEmpty(-1)
                .First();
            if (targetMethodNameArgumentIndex < 0 || targetMethodNameArgumentIndex >= attribute.ConstructorArguments.Length)
            {
                continue;
            }

            string? targetMethodName = attribute.ConstructorArguments[targetMethodNameArgumentIndex].Value?.ToString();
            if (string.IsNullOrEmpty(targetMethodName))
            {
                continue;
            }

            INamedTypeSymbol containingType = methodSymbol.ContainingType;
            IMethodSymbol? partialMethodSymbol = containingType
                .GetMembers(targetMethodName)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(method => method.IsPartialDefinition);

            if (partialMethodSymbol is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratesMethodGeneratorDiagnostics.MissingPartialMethodError,
                    generatorMethod.GetLocation(),
                    targetMethodName,
                    containingType.Name));
                continue;
            }

            validMethods.Add(new GeneratesMethodGenerationTarget(
                generatorMethod,
                methodSymbol,
                targetMethodName,
                partialMethodSymbol,
                containingType));
        }

        return validMethods;
    }
}
