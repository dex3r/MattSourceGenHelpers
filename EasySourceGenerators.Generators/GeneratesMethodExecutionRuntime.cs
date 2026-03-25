using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Generators;

internal sealed record SwitchBodyData(
    IReadOnlyList<(object key, string value)> CasePairs,
    bool HasDefaultCase);

/// <summary>
/// Result extracted from <see cref="DataBuilding.BodyGenerationData"/> after executing a fluent body generator method.
/// </summary>
internal sealed record FluentBodyResult(
    string? ReturnValue,
    bool IsVoid);

internal static class GeneratesMethodExecutionRuntime
{
    internal static (string? value, string? error) ExecuteSimpleGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        IReadOnlyList<IMethodSymbol> allPartials = GetAllUnimplementedPartialMethods(compilation);
        return ExecuteGeneratorMethodWithArgs(generatorMethod, allPartials, compilation, null);
    }

    // SwitchBodyData-based fluent execution has been replaced by the data abstraction layer.
    // Use ExecuteFluentBodyGeneratorMethod instead.

    internal static (FluentBodyResult? result, string? error) ExecuteFluentBodyGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        IReadOnlyList<IMethodSymbol> allPartials = GetAllUnimplementedPartialMethods(compilation);
        CSharpCompilation executableCompilation = BuildExecutionCompilation(allPartials, compilation);

        using MemoryStream stream = new();
        EmitResult emitResult = executableCompilation.Emit(stream);
        if (!emitResult.Success)
        {
            string errors = string.Join("; ", emitResult.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(diagnostic => diagnostic.GetMessage()));
            return (null, $"Compilation failed: {errors}");
        }

        stream.Position = 0;
        AssemblyLoadContext? loadContext = null;
        try
        {
            Dictionary<string, byte[]> compilationReferenceBytes = EmitCompilationReferences(compilation);

            loadContext = new AssemblyLoadContext("__GeneratorExec", isCollectible: true);
            Assembly? capturedAbstractionsAssembly = null;
            loadContext.Resolving += (context, assemblyName) =>
            {
                PortableExecutableReference? match = compilation.References
                    .OfType<PortableExecutableReference>()
                    .FirstOrDefault(reference => reference.FilePath is not null && string.Equals(
                        Path.GetFileNameWithoutExtension(reference.FilePath),
                        assemblyName.Name,
                        StringComparison.OrdinalIgnoreCase));
                if (match?.FilePath != null)
                    return context.LoadFromAssemblyPath(ResolveImplementationAssemblyPath(match.FilePath));

                if (assemblyName.Name != null && compilationReferenceBytes.TryGetValue(assemblyName.Name, out byte[]? bytes))
                {
                    Assembly loaded = context.LoadFromStream(new MemoryStream(bytes));
                    if (string.Equals(assemblyName.Name, Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase))
                        capturedAbstractionsAssembly = loaded;
                    return loaded;
                }

                return null;
            };

            Assembly assembly = loadContext.LoadFromStream(stream);

            MetadataReference[] abstractionsMatchingReferences = compilation.References.Where(reference => reference.Display is not null && (
                    reference.Display.Equals(Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase)
                    || (reference is PortableExecutableReference peRef && peRef.FilePath is not null && Path.GetFileNameWithoutExtension(peRef.FilePath)
                        .Equals(Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase))))
                .ToArray();

            if (abstractionsMatchingReferences.Length == 0)
            {
                MetadataReference[] closestMatches = compilation.References.Where(reference => 
                        reference.Display is not null 
                        && reference.Display.Contains(Consts.SolutionNamespace, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                
                string closestMatchesString = string.Join(", ", closestMatches.Select(reference => reference.Display));
                
                return (null, $"Could not find any reference matching '{Consts.AbstractionsAssemblyName}' in compilation references.\n" +
                              $" Found total references: {compilation.References.Count()}. \nMatching references: {closestMatches.Length}: \n{closestMatchesString}");
            }

            PortableExecutableReference[] peMatchingReferences = abstractionsMatchingReferences.OfType<PortableExecutableReference>().ToArray();
            CompilationReference[] csharpCompilationReference = abstractionsMatchingReferences.OfType<CompilationReference>().ToArray();
            
            Assembly abstractionsAssembly;
            
            if (peMatchingReferences.Length > 0)
            {
                PortableExecutableReference abstractionsReference = peMatchingReferences.First();
                
                if (string.IsNullOrEmpty(abstractionsReference.FilePath))
                {
                    return (null, $"The reference matching '{Consts.AbstractionsAssemblyName}' does not have a valid file path.");
                }
                
                string abstractionsAssemblyPath = ResolveImplementationAssemblyPath(abstractionsReference.FilePath);
                abstractionsAssembly = loadContext.LoadFromAssemblyPath(abstractionsAssemblyPath);
            }
            else if (csharpCompilationReference.Length > 0)
            {
                if (capturedAbstractionsAssembly != null)
                {
                    abstractionsAssembly = capturedAbstractionsAssembly;
                }
                else if (compilationReferenceBytes.TryGetValue(Consts.AbstractionsAssemblyName, out byte[]? abstractionBytes))
                {
                    abstractionsAssembly = loadContext.LoadFromStream(new MemoryStream(abstractionBytes));
                }
                else
                {
                    return (null, $"Found reference matching '{Consts.AbstractionsAssemblyName}' as a CompilationReference, but failed to emit it to a loadable assembly.");
                }
            }
            else
            {
                string matchesString = string.Join(", ", abstractionsMatchingReferences.Select(reference => $"{reference.Display} (type: {reference.GetType().Name})"));
                return (null, $"Found references matching '{Consts.AbstractionsAssemblyName}' but none were PortableExecutableReference or CompilationReference with valid file paths. \nMatching references: {matchesString}");
            }
            
            Type? generatorStaticType = abstractionsAssembly.GetType(Consts.GenerateTypeFullName);
            Type? dataGeneratorsFactoryType = assembly.GetType(Consts.DataGeneratorsFactoryTypeFullName);
            if (generatorStaticType == null || dataGeneratorsFactoryType == null)
            {
                return (null, $"Could not find {Consts.GenerateTypeFullName} or {Consts.DataGeneratorsFactoryTypeFullName} types in compiled assembly");
            }

            object? dataGeneratorsFactory = Activator.CreateInstance(dataGeneratorsFactoryType);
            PropertyInfo? currentGeneratorProperty = generatorStaticType.GetProperty(Consts.CurrentGeneratorPropertyName, BindingFlags.NonPublic | BindingFlags.Static);
            currentGeneratorProperty?.SetValue(null, dataGeneratorsFactory);

            string typeName = generatorMethod.ContainingType.ToDisplayString();
            Type? loadedType = assembly.GetType(typeName);
            if (loadedType == null)
            {
                return (null, $"Could not find type '{typeName}' in compiled assembly");
            }

            MethodInfo? generatorMethodInfo = loadedType.GetMethod(generatorMethod.Name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (generatorMethodInfo == null)
            {
                return (null, $"Could not find method '{generatorMethod.Name}' in type '{typeName}'");
            }

            object? methodResult = generatorMethodInfo.Invoke(null, null);
            if (methodResult == null)
            {
                return (null, "Fluent body generator method returned null");
            }

            return (ExtractBodyGenerationData(methodResult, partialMethod.ReturnType), null);
        }
        catch (Exception ex)
        {
            return (null, $"Error executing generator method '{generatorMethod.Name}': {ex.GetBaseException()}");
        }
        finally
        {
            loadContext?.Unload();
        }
    }

    internal static (string? value, string? error) ExecuteGeneratorMethodWithArgs(
        IMethodSymbol generatorMethod,
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation,
        object?[]? args)
    {
        CSharpCompilation executableCompilation = BuildExecutionCompilation(allPartialMethods, compilation);

        using MemoryStream stream = new();
        EmitResult emitResult = executableCompilation.Emit(stream);
        if (!emitResult.Success)
        {
            string errors = string.Join("; ", emitResult.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(diagnostic => diagnostic.GetMessage()));
            return (null, $"Compilation failed: {errors}");
        }

        stream.Position = 0;
        AssemblyLoadContext? loadContext = null;
        try
        {
            Dictionary<string, byte[]> compilationReferenceBytes = EmitCompilationReferences(compilation);

            loadContext = new AssemblyLoadContext("__GeneratorExec", isCollectible: true);
            loadContext.Resolving += (context, assemblyName) =>
            {
                PortableExecutableReference? match = compilation.References
                    .OfType<PortableExecutableReference>()
                    .FirstOrDefault(reference => reference.FilePath is not null && string.Equals(
                        Path.GetFileNameWithoutExtension(reference.FilePath),
                        assemblyName.Name,
                        StringComparison.OrdinalIgnoreCase));
                if (match?.FilePath != null)
                    return context.LoadFromAssemblyPath(ResolveImplementationAssemblyPath(match.FilePath));

                if (assemblyName.Name != null && compilationReferenceBytes.TryGetValue(assemblyName.Name, out byte[]? bytes))
                    return context.LoadFromStream(new MemoryStream(bytes));

                return null;
            };

            Assembly assembly = loadContext.LoadFromStream(stream);
            string typeName = generatorMethod.ContainingType.ToDisplayString();
            Type? loadedType = assembly.GetType(typeName);
            if (loadedType == null)
            {
                return (null, $"Could not find type '{typeName}' in compiled assembly");
            }

            MethodInfo? generatorMethodInfo = loadedType.GetMethod(generatorMethod.Name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (generatorMethodInfo == null)
            {
                return (null, $"Could not find method '{generatorMethod.Name}' in type '{typeName}'");
            }

            object?[]? convertedArgs = ConvertArguments(args, generatorMethodInfo);
            object? result = generatorMethodInfo.Invoke(null, convertedArgs);
            return (result?.ToString(), null);
        }
        catch (Exception ex)
        {
            return (null, $"Error executing generator method '{generatorMethod.Name}': {ex.GetBaseException()}");
        }
        finally
        {
            loadContext?.Unload();
        }
    }

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

    private static object?[]? ConvertArguments(object?[]? args, MethodInfo methodInfo)
    {
        if (args == null || methodInfo.GetParameters().Length == 0)
        {
            return null;
        }

        Type parameterType = methodInfo.GetParameters()[0].ParameterType;
        return new[] { Convert.ChangeType(args[0], parameterType) };
    }

    private static FluentBodyResult ExtractBodyGenerationData(object methodResult, ITypeSymbol returnType)
    {
        Type resultType = methodResult.GetType();

        // The result should be a DataMethodBodyGenerator containing a BodyGenerationData Data property
        PropertyInfo? dataProperty = resultType.GetProperty(Consts.BodyGenerationDataPropertyName);
        if (dataProperty == null)
        {
            // The method returned something that isn't a DataMethodBodyGenerator.
            // This may happen when the fluent chain is incomplete (e.g., user returned an intermediate builder).
            return new FluentBodyResult(null, returnType.SpecialType == SpecialType.System_Void);
        }

        object? bodyGenerationData = dataProperty.GetValue(methodResult);
        if (bodyGenerationData == null)
        {
            return new FluentBodyResult(null, returnType.SpecialType == SpecialType.System_Void);
        }

        Type dataType = bodyGenerationData.GetType();
        PropertyInfo? returnTypeProperty = dataType.GetProperty("ReturnType");
        Type? dataReturnType = returnTypeProperty?.GetValue(bodyGenerationData) as Type;
        bool isVoid = dataReturnType == typeof(void);

        // Try ReturnConstantValueFactory first
        PropertyInfo? constantFactoryProperty = dataType.GetProperty("ReturnConstantValueFactory");
        Delegate? constantFactory = constantFactoryProperty?.GetValue(bodyGenerationData) as Delegate;
        if (constantFactory != null)
        {
            object? constantValue = constantFactory.DynamicInvoke();
            return new FluentBodyResult(constantValue?.ToString(), isVoid);
        }

        // Try RuntimeDelegateBody
        PropertyInfo? runtimeBodyProperty = dataType.GetProperty("RuntimeDelegateBody");
        Delegate? runtimeBody = runtimeBodyProperty?.GetValue(bodyGenerationData) as Delegate;
        if (runtimeBody != null)
        {
            ParameterInfo[] bodyParams = runtimeBody.Method.GetParameters();
            if (bodyParams.Length == 0)
            {
                object? bodyResult = runtimeBody.DynamicInvoke();
                return new FluentBodyResult(bodyResult?.ToString(), isVoid);
            }

            // For delegates with parameters, we can't invoke at compile time without values
            return new FluentBodyResult(null, isVoid);
        }

        return new FluentBodyResult(null, isVoid);
    }

    private static Dictionary<string, byte[]> EmitCompilationReferences(Compilation compilation)
    {
        Dictionary<string, byte[]> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (CompilationReference compilationRef in compilation.References.OfType<CompilationReference>())
        {
            string assemblyName = compilationRef.Compilation.AssemblyName ?? string.Empty;
            if (string.IsNullOrEmpty(assemblyName))
                continue;
            using MemoryStream refStream = new();
            if (compilationRef.Compilation.Emit(refStream).Success)
                result[assemblyName] = refStream.ToArray();
        }

        return result;
    }

    private static string ResolveImplementationAssemblyPath(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        string? parentDirectory = directory != null ? Path.GetDirectoryName(directory) : null;
        if (directory != null &&
            parentDirectory != null &&
            string.Equals(Path.GetFileName(directory), "ref", StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(parentDirectory, Path.GetFileName(path));
        }

        return path;
    }

    private static CSharpCompilation BuildExecutionCompilation(
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation)
    {
        string dummySource = BuildDummyImplementation(allPartialMethods);
        string dataGeneratorsFactorySource = ReadEmbeddedResource($"{Consts.GeneratorsAssemblyName}.DataGeneratorsFactory.cs");
        string dataMethodBodyBuildersSource = ReadEmbeddedResource($"{Consts.GeneratorsAssemblyName}.DataMethodBodyBuilders.cs");
        string dataRecordsSource = ReadEmbeddedResource($"{Consts.GeneratorsAssemblyName}.DataRecords.cs");
        CSharpParseOptions parseOptions = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                                         ?? CSharpParseOptions.Default;

        return (CSharpCompilation)compilation
            .AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(dummySource, parseOptions),
                CSharpSyntaxTree.ParseText(dataGeneratorsFactorySource, parseOptions),
                CSharpSyntaxTree.ParseText(dataMethodBodyBuildersSource, parseOptions),
                CSharpSyntaxTree.ParseText(dataRecordsSource, parseOptions));
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        using Stream? stream = typeof(GeneratesMethodExecutionRuntime).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found in {Consts.GeneratorsAssemblyName} assembly");
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string BuildDummyImplementation(IEnumerable<IMethodSymbol> partialMethods)
    {
        StringBuilder builder = new();

        IEnumerable<IGrouping<(string? Namespace, string TypeName, bool IsStatic, TypeKind TypeKind), IMethodSymbol>> groupedMethods = partialMethods.GroupBy(
            method => (Namespace: method.ContainingType.ContainingNamespace?.IsGlobalNamespace == false
                    ? method.ContainingType.ContainingNamespace.ToDisplayString()
                    : null,
                TypeName: method.ContainingType.Name,
                IsStatic: method.ContainingType.IsStatic,
                TypeKind: method.ContainingType.TypeKind));

        foreach (IGrouping<(string? Namespace, string TypeName, bool IsStatic, TypeKind TypeKind), IMethodSymbol> typeGroup in groupedMethods)
        {
            string? namespaceName = typeGroup.Key.Namespace;
            if (namespaceName != null)
            {
                builder.AppendLine($"namespace {namespaceName} {{");
            }

            string typeKeyword = typeGroup.Key.TypeKind switch
            {
                TypeKind.Struct => "struct",
                _ => "class"
            };

            string typeModifiers = typeGroup.Key.IsStatic ? "static partial" : "partial";
            builder.AppendLine($"{typeModifiers} {typeKeyword} {typeGroup.Key.TypeName} {{");

            foreach (IMethodSymbol partialMethod in typeGroup)
            {
                string accessibility = partialMethod.DeclaredAccessibility switch
                {
                    Accessibility.Public => "public",
                    Accessibility.Protected => "protected",
                    Accessibility.Internal => "internal",
                    Accessibility.ProtectedOrInternal => "protected internal",
                    Accessibility.ProtectedAndInternal => "private protected",
                    _ => ""
                };

                string staticModifier = partialMethod.IsStatic ? "static " : "";
                string returnType = partialMethod.ReturnType.ToDisplayString();
                string parameters = string.Join(", ", partialMethod.Parameters.Select(parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}"));

                builder.AppendLine($"{accessibility} {staticModifier}partial {returnType} {partialMethod.Name}({parameters}) {{");
                string exceptionName = $"{Consts.AbstractionsAssemblyName}.{nameof(PartialMethodCalledDuringGenerationException)}";
                string throwStatement = $"throw new global::{exceptionName}(\"{partialMethod.Name}\", \"{partialMethod.ContainingType.Name}\");";
                builder.AppendLine(throwStatement);

                builder.AppendLine("}");
            }

            builder.AppendLine("}");

            if (namespaceName != null)
            {
                builder.AppendLine("}");
            }
        }

        return builder.ToString();
    }
}
