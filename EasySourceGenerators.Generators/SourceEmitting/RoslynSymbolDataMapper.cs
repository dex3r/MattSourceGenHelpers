using System;
using System.Collections.Generic;
using System.Linq;
using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators.IncrementalGenerators;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators.SourceEmitting;

/// <summary>
/// Converts Roslyn symbol types to plain data records used by source emitters.
/// This is the thin bridge between Roslyn's type system and the Roslyn-free emitter classes.
/// </summary>
internal static class RoslynSymbolDataMapper
{
    /// <summary>
    /// Converts Roslyn <see cref="INamedTypeSymbol"/> and <see cref="IMethodSymbol"/>
    /// into a <see cref="PartialMethodEmitData"/> record for source emission.
    /// </summary>
    internal static PartialMethodEmitData ToPartialMethodEmitData(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod)
    {
        string? namespaceName = containingType.ContainingNamespace?.IsGlobalNamespace == false
            ? containingType.ContainingNamespace.ToDisplayString()
            : null;

        string typeKeyword = CSharpTypeKeyword.From(containingType.TypeKind);
        string typeModifiers = containingType.IsStatic ? "static partial" : "partial";
        string accessibility = CSharpAccessibilityKeyword.From(partialMethod.DeclaredAccessibility);
        string methodModifiers = partialMethod.IsStatic ? "static partial" : "partial";
        string returnTypeName = partialMethod.ReturnType.ToDisplayString();
        string parameterList = string.Join(", ", partialMethod.Parameters.Select(
            parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}"));

        return new PartialMethodEmitData(
            GeneratorFullName: typeof(GeneratesMethodGenerator).FullName!,
            NamespaceName: namespaceName,
            TypeName: containingType.Name,
            TypeKeyword: typeKeyword,
            TypeModifiers: typeModifiers,
            AccessibilityKeyword: accessibility,
            MethodModifiers: methodModifiers,
            ReturnTypeName: returnTypeName,
            MethodName: partialMethod.Name,
            ParameterList: parameterList,
            ReturnsVoid: partialMethod.ReturnsVoid);
    }

    /// <summary>
    /// Converts a collection of partial method symbols into <see cref="DummyTypeGroupData"/>
    /// records grouped by containing type, suitable for <see cref="DummyImplementationEmitter"/>.
    /// </summary>
    internal static IReadOnlyList<DummyTypeGroupData> ToDummyTypeGroups(IEnumerable<IMethodSymbol> partialMethods)
    {
        List<DummyTypeGroupData> result = new();

        IEnumerable<IGrouping<(string? Namespace, string TypeName, bool IsStatic, TypeKind TypeKind), IMethodSymbol>> groupedMethods =
            partialMethods.GroupBy(method => (
                Namespace: method.ContainingType.ContainingNamespace?.IsGlobalNamespace == false
                    ? method.ContainingType.ContainingNamespace.ToDisplayString()
                    : null,
                TypeName: method.ContainingType.Name,
                IsStatic: method.ContainingType.IsStatic,
                TypeKind: method.ContainingType.TypeKind));

        foreach (IGrouping<(string? Namespace, string TypeName, bool IsStatic, TypeKind TypeKind), IMethodSymbol> typeGroup in groupedMethods)
        {
            string typeKeyword = CSharpTypeKeyword.From(typeGroup.Key.TypeKind);
            string typeModifiers = typeGroup.Key.IsStatic ? "static partial" : "partial";

            List<DummyMethodData> methods = new();
            foreach (IMethodSymbol partialMethod in typeGroup)
            {
                string accessibility = CSharpAccessibilityKeyword.FromOrEmpty(partialMethod.DeclaredAccessibility);
                string methodModifiers = partialMethod.IsStatic ? "static partial" : "partial";
                string returnTypeName = partialMethod.ReturnType.ToDisplayString();
                string parameterList = string.Join(", ", partialMethod.Parameters.Select(
                    parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}"));

                string exceptionFullName = $"{Consts.AbstractionsAssemblyName}.{nameof(PartialMethodCalledDuringGenerationException)}";
                string bodyStatement = $"throw new global::{exceptionFullName}(\"{partialMethod.Name}\", \"{partialMethod.ContainingType.Name}\");";

                methods.Add(new DummyMethodData(
                    AccessibilityKeyword: accessibility,
                    MethodModifiers: methodModifiers,
                    ReturnTypeName: returnTypeName,
                    MethodName: partialMethod.Name,
                    ParameterList: parameterList,
                    BodyStatement: bodyStatement));
            }

            result.Add(new DummyTypeGroupData(
                NamespaceName: typeGroup.Key.Namespace,
                TypeName: typeGroup.Key.TypeName,
                TypeKeyword: typeKeyword,
                TypeModifiers: typeModifiers,
                Methods: methods));
        }

        return result;
    }
}
