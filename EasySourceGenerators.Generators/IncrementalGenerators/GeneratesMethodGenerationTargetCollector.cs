using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EasySourceGenerators.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static EasySourceGenerators.Generators.Consts;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Represents a validated generation target: a generator method with its resolved
/// target partial method and containing type information.
/// </summary>
internal sealed record GeneratesMethodGenerationTarget(
    MethodDeclarationSyntax Syntax,
    IMethodSymbol Symbol,
    string TargetMethodName,
    IMethodSymbol PartialMethod,
    INamedTypeSymbol ContainingType);

/// <summary>
/// Collects and validates generator methods marked with <c>[MethodBodyGenerator]</c>,
/// resolving each to its target partial method and reporting diagnostics for invalid configurations.
/// </summary>
internal static class GeneratesMethodGenerationTargetCollector
{
    /// <summary>
    /// Scans all generator methods, validates their configuration, and returns a list of
    /// valid generation targets. Reports diagnostics for non-static generators (MSGH002)
    /// and missing partial methods (MSGH001).
    /// </summary>
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
            int targetMethodNameArgumentIndex = -1;
            for (int parameterIndex = 0; parameterIndex < constructorParameters.Length; parameterIndex++)
            {
                IParameterSymbol constructorParameter = constructorParameters[parameterIndex];
                if (constructorParameter.Name.Equals(nameof(MethodBodyGenerator.SameClassMethodName), StringComparison.OrdinalIgnoreCase))
                {
                    targetMethodNameArgumentIndex = parameterIndex;
                    break;
                }
            }
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
                Location? attributeLocation = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratesMethodGeneratorDiagnostics.MissingPartialMethodError,
                    attributeLocation ?? generatorMethod.GetLocation(),
                    targetMethodName,
                    containingType.Name));
                continue;
            }

            string generatorReturnType = methodSymbol.ReturnType.ToDisplayString();
            string targetReturnType = partialMethodSymbol.ReturnType.ToDisplayString();

            if (generatorReturnType != IMethodImplementationGeneratorFullName && generatorReturnType != targetReturnType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratesMethodGeneratorDiagnostics.MethodBodyGeneratorInvalidReturnType,
                    generatorMethod.ReturnType.GetLocation(),
                    methodSymbol.Name));
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
