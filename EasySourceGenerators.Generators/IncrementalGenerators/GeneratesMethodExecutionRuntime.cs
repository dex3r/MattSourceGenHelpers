using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

internal sealed record SwitchBodyData(
    IReadOnlyList<(object key, string value)> CasePairs,
    bool HasDefaultCase);

/// <summary>
/// Result extracted from <see cref="DataBuilding.BodyGenerationData"/> after executing a fluent body generator method.
/// <see cref="HasDelegateBody"/> indicates that the generator used <c>UseProvidedBody</c>,
/// signaling that the delegate body source code should be extracted from the syntax tree.
/// </summary>
internal sealed record FluentBodyResult(
    string? ReturnValue,
    bool IsVoid,
    bool HasDelegateBody);

/// <summary>
/// Orchestrates the execution of generator methods at compile time.
/// Delegates compilation to <see cref="GeneratorAssemblyExecutor"/>,
/// abstractions resolution to <see cref="AbstractionsAssemblyResolver"/>,
/// factory setup to <see cref="DataGeneratorsFactorySetup"/>,
/// and data extraction to <see cref="BodyGenerationDataExtractor"/>.
/// </summary>
internal static class GeneratesMethodExecutionRuntime
{
    /// <summary>
    /// Executes a simple (non-fluent) generator method with no arguments and returns its string result.
    /// </summary>
    internal static (string? value, string? error) ExecuteSimpleGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        IReadOnlyList<IMethodSymbol> allPartials = GetAllUnimplementedPartialMethods(compilation);
        return ExecuteGeneratorMethodWithArgs(generatorMethod, allPartials, compilation, null);
    }

    /// <summary>
    /// Executes a fluent body generator method and extracts the <see cref="FluentBodyResult"/>
    /// from the returned <c>DataMethodBodyGenerator</c>.
    /// </summary>
    internal static (FluentBodyResult? result, string? error) ExecuteFluentBodyGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        IReadOnlyList<IMethodSymbol> allPartials = GetAllUnimplementedPartialMethods(compilation);

        (LoadedAssemblyContext? loadedContext, string? loadError) =
            GeneratorAssemblyExecutor.CompileAndLoadAssembly(allPartials, compilation);
        if (loadError != null)
        {
            return (null, loadError);
        }

        using LoadedAssemblyContext context = loadedContext!;
        try
        {
            (Assembly? abstractionsAssembly, string? abstractionsError) =
                AbstractionsAssemblyResolver.Resolve(context, compilation);
            if (abstractionsError != null)
            {
                return (null, abstractionsError);
            }

            string? setupError = DataGeneratorsFactorySetup.Setup(context.Assembly, abstractionsAssembly!);
            if (setupError != null)
            {
                return (null, setupError);
            }

            (object? methodResult, string? invokeError) =
                InvokeStaticMethod(context.Assembly, generatorMethod);
            if (invokeError != null)
            {
                return (null, invokeError);
            }

            if (methodResult == null)
            {
                return (null, "Fluent body generator method returned null");
            }

            bool isVoidReturnType = partialMethod.ReturnType.SpecialType == SpecialType.System_Void;
            FluentBodyResult bodyResult = BodyGenerationDataExtractor.Extract(methodResult, isVoidReturnType);
            return (bodyResult, null);
        }
        catch (Exception ex)
        {
            return (null, $"Error executing generator method '{generatorMethod.Name}': {ex.GetBaseException()}");
        }
    }

    /// <summary>
    /// Executes a generator method with optional arguments and returns its string result.
    /// </summary>
    internal static (string? value, string? error) ExecuteGeneratorMethodWithArgs(
        IMethodSymbol generatorMethod,
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation,
        object?[]? args)
    {
        (LoadedAssemblyContext? loadedContext, string? loadError) =
            GeneratorAssemblyExecutor.CompileAndLoadAssembly(allPartialMethods, compilation);
        if (loadError != null)
        {
            return (null, loadError);
        }

        using LoadedAssemblyContext context = loadedContext!;
        try
        {
            string typeName = generatorMethod.ContainingType.ToDisplayString();
            (Type? loadedType, string? typeError) = GeneratorAssemblyExecutor.FindType(context.Assembly, typeName);
            if (typeError != null)
            {
                return (null, typeError);
            }

            (MethodInfo? methodInfo, string? methodError) =
                GeneratorAssemblyExecutor.FindStaticMethod(loadedType!, generatorMethod.Name, typeName);
            if (methodError != null)
            {
                return (null, methodError);
            }

            object?[]? convertedArgs = GeneratorAssemblyExecutor.ConvertArguments(args, methodInfo!);
            object? result = methodInfo!.Invoke(null, convertedArgs);
            return (result?.ToString(), null);
        }
        catch (Exception ex)
        {
            return (null, $"Error executing generator method '{generatorMethod.Name}': {ex.GetBaseException()}");
        }
    }

    /// <summary>
    /// Finds all unimplemented partial method definitions across all syntax trees in the compilation.
    /// </summary>
    internal static IReadOnlyList<IMethodSymbol> GetAllUnimplementedPartialMethods(Compilation compilation)
    {
        List<IMethodSymbol> methods = new();
        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            IEnumerable<MethodDeclarationSyntax> partialMethodDeclarations = syntaxTree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)));

            foreach (MethodDeclarationSyntax declaration in partialMethodDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(declaration) is IMethodSymbol symbol && symbol.IsPartialDefinition)
                {
                    methods.Add(symbol);
                }
            }
        }

        return methods;
    }

    /// <summary>
    /// Locates and invokes a static generator method in the loaded assembly.
    /// </summary>
    private static (object? result, string? error) InvokeStaticMethod(
        Assembly assembly,
        IMethodSymbol generatorMethod)
    {
        string typeName = generatorMethod.ContainingType.ToDisplayString();
        (Type? loadedType, string? typeError) = GeneratorAssemblyExecutor.FindType(assembly, typeName);
        if (typeError != null)
        {
            return (null, typeError);
        }

        (MethodInfo? methodInfo, string? methodError) =
            GeneratorAssemblyExecutor.FindStaticMethod(loadedType!, generatorMethod.Name, typeName);
        if (methodError != null)
        {
            return (null, methodError);
        }

        object? result = methodInfo!.Invoke(null, null);
        return (result, null);
    }
}
