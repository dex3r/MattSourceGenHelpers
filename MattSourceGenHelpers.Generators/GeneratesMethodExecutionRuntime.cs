using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace MattSourceGenHelpers.Generators;

internal sealed record SwitchBodyData(
    IReadOnlyList<(object key, string value)> CasePairs,
    bool HasDefaultCase);

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

    internal static (SwitchBodyData? record, string? error) ExecuteFluentGeneratorMethod(
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
            loadContext = new AssemblyLoadContext("__GeneratorExec", isCollectible: true);
            loadContext.Resolving += (context, assemblyName) =>
            {
                PortableExecutableReference? match = compilation.References
                    .OfType<PortableExecutableReference>()
                    .FirstOrDefault(reference => reference.FilePath is not null && string.Equals(
                        Path.GetFileNameWithoutExtension(reference.FilePath),
                        assemblyName.Name,
                        StringComparison.OrdinalIgnoreCase));
                return match?.FilePath != null
                    ? context.LoadFromAssemblyPath(ResolveImplementationAssemblyPath(match.FilePath))
                    : null;
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
            else if(csharpCompilationReference.Length > 0)
            {
                //TODO: Fix
                return (null, $"Found reference matching '{Consts.AbstractionsAssemblyName}' as a CompilationReference, but executing generator methods currently requires a PortableExecutableReference with a valid file path to load the assembly.");
            }
            else
            {
                string matchesString = string.Join(", ", abstractionsMatchingReferences.Select(reference => $"{reference.Display} (type: {reference.GetType().Name})"));
                return (null, $"Found references matching '{Consts.AbstractionsAssemblyName}' but none were PortableExecutableReference or CompilationReference with valid file paths. \nMatching references: {matchesString}");
            }
            
            Type? generatorStaticType = abstractionsAssembly.GetType(Consts.GenerateTypeFullName);
            Type? recordingFactoryType = abstractionsAssembly.GetType(Consts.RecordingGeneratorsFactoryTypeFullName);
            if (generatorStaticType == null || recordingFactoryType == null)
            {
                return (null, $"Could not find {Consts.GenerateTypeFullName} or {Consts.RecordingGeneratorsFactoryTypeFullName} types in Abstractions assembly");
            }

            object? recordingFactory = Activator.CreateInstance(recordingFactoryType);
            PropertyInfo? currentGeneratorProperty = generatorStaticType.GetProperty(Consts.CurrentGeneratorPropertyName, BindingFlags.Public | BindingFlags.Static);
            currentGeneratorProperty?.SetValue(null, recordingFactory);

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

            generatorMethodInfo.Invoke(null, null);

            PropertyInfo? lastRecordProperty = recordingFactoryType.GetProperty(Consts.LastRecordPropertyName);
            object? lastRecord = lastRecordProperty?.GetValue(recordingFactory);
            if (lastRecord == null)
            {
                return (null, "RecordingGeneratorsFactory did not produce a record");
            }

            return (ExtractSwitchBodyData(lastRecord, partialMethod.ReturnType), null);
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
            loadContext = new AssemblyLoadContext("__GeneratorExec", isCollectible: true);
            loadContext.Resolving += (context, assemblyName) =>
            {
                PortableExecutableReference? match = compilation.References
                    .OfType<PortableExecutableReference>()
                    .FirstOrDefault(reference => reference.FilePath is not null && string.Equals(
                        Path.GetFileNameWithoutExtension(reference.FilePath),
                        assemblyName.Name,
                        StringComparison.OrdinalIgnoreCase));
                return match?.FilePath != null
                    ? context.LoadFromAssemblyPath(ResolveImplementationAssemblyPath(match.FilePath))
                    : null;
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

    private static SwitchBodyData ExtractSwitchBodyData(object lastRecord, ITypeSymbol returnType)
    {
        Type recordType = lastRecord.GetType();
        PropertyInfo? caseKeysProperty = recordType.GetProperty("CaseKeys");
        PropertyInfo? caseValuesProperty = recordType.GetProperty("CaseValues");
        PropertyInfo? hasDefaultProperty = recordType.GetProperty("HasDefaultCase");

        IList caseKeys = (caseKeysProperty?.GetValue(lastRecord) as IList) ?? new List<object>();
        IList caseValues = (caseValuesProperty?.GetValue(lastRecord) as IList) ?? new List<object?>();
        bool hasDefaultCase = (bool)(hasDefaultProperty?.GetValue(lastRecord) ?? false);

        List<(object key, string value)> pairs = new();
        for (int index = 0; index < caseKeys.Count; index++)
        {
            object key = caseKeys[index]!;
            string? value = index < caseValues.Count ? caseValues[index]?.ToString() : null;
            pairs.Add((key, GeneratesMethodPatternSourceBuilder.FormatValueAsCSharpLiteral(value, returnType)));
        }

        return new SwitchBodyData(pairs, hasDefaultCase);
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
        CSharpParseOptions parseOptions = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                                         ?? CSharpParseOptions.Default;

        return (CSharpCompilation)compilation
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(dummySource, parseOptions));
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
                if (!partialMethod.ReturnsVoid)
                {
                    builder.AppendLine("return default!;");
                }

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
