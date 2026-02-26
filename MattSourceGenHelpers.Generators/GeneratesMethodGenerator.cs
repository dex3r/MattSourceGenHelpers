using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;

namespace MattSourceGenHelpers.Generators;

[Generator]
public class GeneratesMethodGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor MissingPartialMethodError = new(
        id: "MSGH001",
        title: "Missing partial method",
        messageFormat: "Could not find partial method '{0}' in class '{1}'",
        category: "GeneratesMethodGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor GeneratorMethodMustBeStaticError = new(
        id: "MSGH002",
        title: "Generator method must be static",
        messageFormat: "Method '{0}' marked with [GeneratesMethod] must be static",
        category: "GeneratesMethodGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor GeneratingMethodInfo = new(
        id: "MSGH003",
        title: "Generating partial method implementation",
        messageFormat: "Generating implementation for partial method '{0}' in class '{1}' using generator '{2}'",
        category: "GeneratesMethodGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: false);

    private static readonly DiagnosticDescriptor GeneratorMethodExecutionError = new(
        id: "MSGH004",
        title: "Generator method execution failed",
        messageFormat: "Failed to execute generator method '{0}': {1}",
        category: "GeneratesMethodGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private record GeneratorMethodInfo(
        MethodDeclarationSyntax Syntax,
        IMethodSymbol Symbol,
        string TargetMethodName,
        IMethodSymbol PartialMethod,
        INamedTypeSymbol ContainingType);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodsWithAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsMethodWithGeneratesMethodAttribute,
                transform: GetMethodDeclaration)
            .Where(m => m != null)
            .Collect();

        context.RegisterSourceOutput(
            methodsWithAttribute.Combine(context.CompilationProvider),
            (ctx, data) => Execute(ctx, data.Left, data.Right));
    }

    private static bool IsMethodWithGeneratesMethodAttribute(SyntaxNode node, CancellationToken _)
    {
        if (node is not MethodDeclarationSyntax method)
            return false;

        return method.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => a.Name.ToString() is "GeneratesMethod" or "GeneratesMethodAttribute");
    }

    private static MethodDeclarationSyntax? GetMethodDeclaration(GeneratorSyntaxContext context, CancellationToken _)
    {
        return context.Node as MethodDeclarationSyntax;
    }

    private void Execute(
        SourceProductionContext context,
        ImmutableArray<MethodDeclarationSyntax?> generatorMethods,
        Compilation compilation)
    {
        var validMethods = new List<GeneratorMethodInfo>();

        foreach (var generatorMethod in generatorMethods)
        {
            if (generatorMethod == null)
                continue;

            var semanticModel = compilation.GetSemanticModel(generatorMethod.SyntaxTree);
            var methodSymbol = semanticModel.GetDeclaredSymbol(generatorMethod) as IMethodSymbol;

            if (methodSymbol == null)
                continue;

            if (!methodSymbol.IsStatic)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratorMethodMustBeStaticError,
                    generatorMethod.GetLocation(),
                    generatorMethod.Identifier.Text));
                continue;
            }

            var attribute = methodSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "MattSourceGenHelpers.Abstractions.GeneratesMethod");

            if (attribute == null || attribute.ConstructorArguments.Length == 0)
                continue;

            var targetMethodName = attribute.ConstructorArguments[0].Value?.ToString();
            if (string.IsNullOrEmpty(targetMethodName))
                continue;

            var containingType = methodSymbol.ContainingType;
            var partialMethodSymbol = containingType.GetMembers(targetMethodName)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.IsPartialDefinition);

            if (partialMethodSymbol == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MissingPartialMethodError,
                    generatorMethod.GetLocation(),
                    targetMethodName,
                    containingType.Name));
                continue;
            }

            validMethods.Add(new GeneratorMethodInfo(generatorMethod, methodSymbol, targetMethodName, partialMethodSymbol, containingType));
        }

        // Group by (containing type display string, target method name)
        var groups = validMethods
            .GroupBy(m => (TypeKey: m.ContainingType.ToDisplayString(), m.TargetMethodName));

        // Collect all unimplemented partial methods once for the whole compilation
        var allPartials = GetAllUnimplementedPartialMethods(compilation);

        foreach (var group in groups)
        {
            var methods = group.ToList();
            var first = methods[0];

            context.ReportDiagnostic(Diagnostic.Create(
                GeneratingMethodInfo,
                first.Syntax.GetLocation(),
                first.TargetMethodName,
                first.ContainingType.Name,
                string.Join(", ", methods.Select(m => m.Symbol.Name))));

            // Check if this group uses the attribute-based switch pattern
            bool hasSwitchCase = methods.Any(m => m.Symbol.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == "MattSourceGenHelpers.Abstractions.SwitchCase"));
            bool hasSwitchDefault = methods.Any(m => m.Symbol.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == "MattSourceGenHelpers.Abstractions.SwitchDefault"));

            // Check if this is a fluent pattern (returns IMethodImplementationGenerator)
            bool isFluentPattern = methods.Count == 1 &&
                methods[0].Symbol.ReturnType.ToDisplayString() == "MattSourceGenHelpers.Abstractions.IMethodImplementationGenerator";

            string source;
            if (hasSwitchCase || hasSwitchDefault)
            {
                source = GenerateFromSwitchAttributes(context, methods, first.PartialMethod, first.ContainingType, allPartials, compilation);
            }
            else if (isFluentPattern)
            {
                source = GenerateFromFluent(context, methods[0], first.PartialMethod, first.ContainingType, compilation);
            }
            else
            {
                // Simple pattern: execute first method and use returned value
                var (returnValue, error) = ExecuteSimpleGeneratorMethod(first.Symbol, first.PartialMethod, compilation);
                if (error != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GeneratorMethodExecutionError,
                        first.Syntax.GetLocation(),
                        first.Symbol.Name,
                        error));
                    continue;
                }
                source = GenerateSimplePartialMethod(first.ContainingType, first.PartialMethod, returnValue);
            }

            if (!string.IsNullOrEmpty(source))
                context.AddSource($"{first.ContainingType.Name}_{first.TargetMethodName}.g.cs", source);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Attribute-based switch pattern  ([SwitchCase] / [SwitchDefault])
    // ──────────────────────────────────────────────────────────────────────────

    private static string GenerateFromSwitchAttributes(
        SourceProductionContext context,
        List<GeneratorMethodInfo> methods,
        IMethodSymbol partialMethod,
        INamedTypeSymbol containingType,
        IReadOnlyList<IMethodSymbol> allPartials,
        Compilation compilation)
    {
        var switchCaseMethods = methods.Where(m => m.Symbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == "MattSourceGenHelpers.Abstractions.SwitchCase")).ToList();
        var switchDefaultMethod = methods.FirstOrDefault(m => m.Symbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == "MattSourceGenHelpers.Abstractions.SwitchDefault"));

        var cases = new List<(object key, string value)>();

        // For each [SwitchCase] method, execute it for each case value
        foreach (var switchMethod in switchCaseMethods)
        {
            var switchCaseAttrs = switchMethod.Symbol.GetAttributes()
                .Where(a => a.AttributeClass?.ToDisplayString() == "MattSourceGenHelpers.Abstractions.SwitchCase");

            foreach (var attr in switchCaseAttrs)
            {
                if (attr.ConstructorArguments.Length == 0) continue;
                var caseArg = attr.ConstructorArguments[0].Value;
                if (caseArg == null) continue;

                var (result, error) = ExecuteGeneratorMethodWithArgs(switchMethod.Symbol, allPartials, compilation, new[] { caseArg });
                if (error != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GeneratorMethodExecutionError,
                        switchMethod.Syntax.GetLocation(),
                        switchMethod.Symbol.Name,
                        error));
                    continue;
                }

                cases.Add((caseArg, FormatCaseValue(result, partialMethod.ReturnType)));
            }
        }

        // Extract default expression from the [SwitchDefault] method's syntax
        string? defaultExpression = null;
        if (switchDefaultMethod != null)
            defaultExpression = ExtractDefaultExpressionFromSwitchDefaultMethod(switchDefaultMethod.Syntax);

        return GenerateSwitchMethodSource(containingType, partialMethod, cases, defaultExpression);
    }

    /// <summary>
    /// Extracts the body expression from a [SwitchDefault] method whose body is a lambda.
    /// e.g. "decimalNumber => SlowMath.CalculatePiDecimal(decimalNumber)" → "SlowMath.CalculatePiDecimal(decimalNumber)"
    /// </summary>
    private static string? ExtractDefaultExpressionFromSwitchDefaultMethod(MethodDeclarationSyntax method)
    {
        // Expression body: => <expr>
        var bodyExpr = method.ExpressionBody?.Expression;
        if (bodyExpr == null && method.Body != null)
        {
            // Block body: { return <expr>; }
            var returnStmt = method.Body.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
            bodyExpr = returnStmt?.Expression;
        }
        return ExtractInnermostLambdaBody(bodyExpr);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Fluent pattern  (returns IMethodImplementationGenerator)
    // ──────────────────────────────────────────────────────────────────────────

    private static string GenerateFromFluent(
        SourceProductionContext context,
        GeneratorMethodInfo methodInfo,
        IMethodSymbol partialMethod,
        INamedTypeSymbol containingType,
        Compilation compilation)
    {
        var (record, error) = ExecuteFluentGeneratorMethod(methodInfo.Symbol, partialMethod, compilation);
        if (error != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GeneratorMethodExecutionError,
                methodInfo.Syntax.GetLocation(),
                methodInfo.Symbol.Name,
                error));
            return string.Empty;
        }

        var cases = record!;

        // Extract default expression from the RuntimeBody or CompileTimeBody call in the method syntax
        string? defaultExpression = null;
        if (cases.HasDefaultCase)
            defaultExpression = ExtractDefaultExpressionFromFluentMethod(methodInfo.Syntax);

        return GenerateSwitchMethodSource(containingType, partialMethod, cases.CasePairs, defaultExpression);
    }

    /// <summary>
    /// Finds RuntimeBody(...) or CompileTimeBody(...) in the ForDefaultCase() chain
    /// and extracts the innermost lambda body expression string.
    /// </summary>
    private static string? ExtractDefaultExpressionFromFluentMethod(MethodDeclarationSyntax method)
    {
        // Walk all InvocationExpressionSyntax nodes; find the one named RuntimeBody or CompileTimeBody
        // that follows a ForDefaultCase() call.
        var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (var inv in invocations)
        {
            if (inv.Expression is not MemberAccessExpressionSyntax ma) continue;
            var name = ma.Name.Identifier.Text;
            if (name is not ("RuntimeBody" or "CompileTimeBody")) continue;

            var arg = inv.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            return ExtractInnermostLambdaBody(arg);
        }
        return null;
    }

    /// <summary>
    /// Recursively unwraps nested lambdas and returns the body of the innermost one.
    /// e.g. "x => () => Foo(x)"  →  "Foo(x)"
    ///      "x => Foo(x)"        →  "Foo(x)"
    /// </summary>
    private static string? ExtractInnermostLambdaBody(ExpressionSyntax? expr)
    {
        while (true)
        {
            switch (expr)
            {
                case SimpleLambdaExpressionSyntax simple:
                    expr = simple.Body as ExpressionSyntax;
                    break;
                case ParenthesizedLambdaExpressionSyntax paren:
                    expr = paren.Body as ExpressionSyntax;
                    break;
                default:
                    return expr?.ToString();
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Source generation helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static string GenerateSwitchMethodSource(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        IReadOnlyList<(object key, string value)> cases,
        string? defaultExpression)
    {
        var sb = new StringBuilder();

        AppendNamespaceAndTypeHeader(sb, containingType, partialMethod);

        var paramName = partialMethod.Parameters.Length > 0 ? partialMethod.Parameters[0].Name : "arg";

        sb.AppendLine($"        switch ({paramName})");
        sb.AppendLine("        {");

        foreach (var (key, value) in cases)
            sb.AppendLine($"            case {key}: return {value};");

        if (defaultExpression != null)
            sb.AppendLine($"            default: return {defaultExpression};");

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void AppendNamespaceAndTypeHeader(StringBuilder sb, INamedTypeSymbol containingType, IMethodSymbol partialMethod)
    {
        var namespaceName = containingType.ContainingNamespace?.IsGlobalNamespace == false
            ? containingType.ContainingNamespace.ToDisplayString()
            : null;

        if (namespaceName != null)
        {
            sb.AppendLine($"namespace {namespaceName};");
            sb.AppendLine();
        }

        var typeKeyword = containingType.TypeKind switch
        {
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            _ => "class"
        };

        var typeModifiers = containingType.IsStatic ? "static partial" : "partial";
        sb.AppendLine($"{typeModifiers} {typeKeyword} {containingType.Name}");
        sb.AppendLine("{");

        var accessibility = partialMethod.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private"
        };

        var returnTypeName = partialMethod.ReturnType.ToDisplayString();
        var methodName = partialMethod.Name;
        var parameters = string.Join(", ", partialMethod.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));

        var methodModifiers = partialMethod.IsStatic ? "static partial" : "partial";
        sb.AppendLine($"    {accessibility} {methodModifiers} {returnTypeName} {methodName}({parameters})");
        sb.AppendLine("    {");
    }

    private static string FormatCaseValue(string? value, ITypeSymbol returnType)
    {
        if (value == null) return "default";
        return returnType.SpecialType switch
        {
            SpecialType.System_String => SyntaxFactory.Literal(value).Text,
            SpecialType.System_Char when value.Length == 1 => SyntaxFactory.Literal(value[0]).Text,
            SpecialType.System_Boolean => value.ToLowerInvariant(),
            _ => value
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Simple pattern (existing behaviour)
    // ──────────────────────────────────────────────────────────────────────────

    private static string GenerateSimplePartialMethod(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        string? returnValue)
    {
        var sb = new StringBuilder();

        AppendNamespaceAndTypeHeader(sb, containingType, partialMethod);

        if (!partialMethod.ReturnsVoid)
        {
            var literal = FormatCaseValue(returnValue, partialMethod.ReturnType);
            sb.AppendLine($"        return {literal};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Compilation / execution helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static (string? value, string? error) ExecuteSimpleGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        var allPartials = GetAllUnimplementedPartialMethods(compilation);
        var (result, error) = ExecuteGeneratorMethodWithArgs(generatorMethod, allPartials, compilation, null);
        return (result, error);
    }

    /// <summary>
    /// Holds the recorded switch body data extracted via reflection from the loaded assembly.
    /// </summary>
    private record SwitchBodyData(
        IReadOnlyList<(object key, string value)> CasePairs,
        bool HasDefaultCase);

    private static (SwitchBodyData? record, string? error) ExecuteFluentGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        var allPartials = GetAllUnimplementedPartialMethods(compilation);
        var dllCompilation = BuildExecutionCompilation(allPartials, compilation);

        using var ms = new MemoryStream();
        var emitResult = dllCompilation.Emit(ms);
        if (!emitResult.Success)
        {
            var errors = string.Join("; ", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage()));
            return (null, $"Compilation failed: {errors}");
        }

        ms.Position = 0;
        AssemblyLoadContext? loadContext = null;
        try
        {
            loadContext = new AssemblyLoadContext("__GeneratorExec", isCollectible: true);
            loadContext.Resolving += (ctx, assemblyName) =>
            {
                var match = compilation.References
                    .OfType<PortableExecutableReference>()
                    .FirstOrDefault(r => string.Equals(
                        Path.GetFileNameWithoutExtension(r.FilePath),
                        assemblyName.Name,
                        StringComparison.OrdinalIgnoreCase));
                return match?.FilePath != null ? ctx.LoadFromAssemblyPath(match.FilePath) : null;
            };

            var assembly = loadContext.LoadFromStream(ms);

            // The Generator and RecordingGeneratorsFactory types are in the Abstractions assembly
            // (a referenced assembly), not in the compiled user code assembly.
            // The compilation reference might point to a reference assembly (metadata-only),
            // so we try to find the actual implementation DLL.
            var abstractionsRef = compilation.References
                .OfType<PortableExecutableReference>()
                .FirstOrDefault(r => string.Equals(
                    Path.GetFileNameWithoutExtension(r.FilePath),
                    "MattSourceGenHelpers.Abstractions",
                    StringComparison.OrdinalIgnoreCase));

            if (abstractionsRef?.FilePath == null)
                return (null, "Could not find MattSourceGenHelpers.Abstractions reference in compilation");

            // If path is a reference assembly (in a "ref" subdirectory), resolve the implementation DLL
            var abstractionsPath = ResolveImplementationAssemblyPath(abstractionsRef.FilePath);

            var abstractionsAssembly = loadContext.LoadFromAssemblyPath(abstractionsPath);

            // Set Generator.CurrentGenerator to a fresh RecordingGeneratorsFactory in the loaded assembly
            var generatorStaticType = abstractionsAssembly.GetType("MattSourceGenHelpers.Abstractions.Generator");
            var recordingFactoryType = abstractionsAssembly.GetType("MattSourceGenHelpers.Abstractions.RecordingGeneratorsFactory");

            if (generatorStaticType == null || recordingFactoryType == null)
                return (null, "Could not find Generator or RecordingGeneratorsFactory types in Abstractions assembly");

            var recordingFactory = Activator.CreateInstance(recordingFactoryType);
            var currentGeneratorProp = generatorStaticType.GetProperty("CurrentGenerator",
                BindingFlags.Public | BindingFlags.Static);
            currentGeneratorProp?.SetValue(null, recordingFactory);

            // Execute the generator method
            var typeName = generatorMethod.ContainingType.ToDisplayString();
            var type = assembly.GetType(typeName);
            if (type == null)
                return (null, $"Could not find type '{typeName}' in compiled assembly");

            var method = type.GetMethod(generatorMethod.Name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
                return (null, $"Could not find method '{generatorMethod.Name}' in type '{typeName}'");

            method.Invoke(null, null);

            // Read the recorded switch body from the factory
            var lastRecordProp = recordingFactoryType.GetProperty("LastRecord");
            var lastRecord = lastRecordProp?.GetValue(recordingFactory);
            if (lastRecord == null)
                return (null, "RecordingGeneratorsFactory did not produce a record");

            var recordType = lastRecord.GetType();
            var caseKeysProp = recordType.GetProperty("CaseKeys");
            var caseValuesProp = recordType.GetProperty("CaseValues");
            var hasDefaultProp = recordType.GetProperty("HasDefaultCase");

            var caseKeys = (caseKeysProp?.GetValue(lastRecord) as IList) ?? new List<object>();
            var caseValues = (caseValuesProp?.GetValue(lastRecord) as IList) ?? new List<object?>();
            var hasDefault = (bool)(hasDefaultProp?.GetValue(lastRecord) ?? false);

            var pairs = new List<(object, string)>();
            for (int i = 0; i < caseKeys.Count; i++)
            {
                var k = caseKeys[i]!;
                var v = i < caseValues.Count ? caseValues[i]?.ToString() : null;
                pairs.Add((k, FormatCaseValue(v, partialMethod.ReturnType)));
            }

            return (new SwitchBodyData(pairs, hasDefault), null);
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

    private static (string? value, string? error) ExecuteGeneratorMethodWithArgs(
        IMethodSymbol generatorMethod,
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation,
        object?[]? args)
    {
        var dllCompilation = BuildExecutionCompilation(allPartialMethods, compilation);

        using var ms = new MemoryStream();
        var emitResult = dllCompilation.Emit(ms);

        if (!emitResult.Success)
        {
            var errors = string.Join("; ", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage()));
            return (null, $"Compilation failed: {errors}");
        }

        ms.Position = 0;
        AssemblyLoadContext? loadContext = null;
        try
        {
            loadContext = new AssemblyLoadContext("__GeneratorExec", isCollectible: true);
            loadContext.Resolving += (ctx, assemblyName) =>
            {
                var match = compilation.References
                    .OfType<PortableExecutableReference>()
                    .FirstOrDefault(r => string.Equals(
                        Path.GetFileNameWithoutExtension(r.FilePath),
                        assemblyName.Name,
                        StringComparison.OrdinalIgnoreCase));
                return match?.FilePath != null ? ctx.LoadFromAssemblyPath(match.FilePath) : null;
            };

            var assembly = loadContext.LoadFromStream(ms);
            var typeName = generatorMethod.ContainingType.ToDisplayString();
            var type = assembly.GetType(typeName);

            if (type == null)
                return (null, $"Could not find type '{typeName}' in compiled assembly");

            var method = type.GetMethod(
                generatorMethod.Name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
                return (null, $"Could not find method '{generatorMethod.Name}' in type '{typeName}'");

            // Convert args to match the method's parameter types
            object?[]? convertedArgs = null;
            if (args != null && method.GetParameters().Length > 0)
            {
                var paramType = method.GetParameters()[0].ParameterType;
                convertedArgs = new[] { Convert.ChangeType(args[0], paramType) };
            }

            var result = method.Invoke(null, convertedArgs);
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

    /// <summary>
    /// If the given path is a reference assembly (located in a "ref" subdirectory),
    /// returns the path to the corresponding implementation assembly.
    /// </summary>
    private static string ResolveImplementationAssemblyPath(string path)
    {
        // Reference assemblies are often placed in a "ref" subdirectory
        // e.g. .../bin/Debug/net10.0/ref/Foo.dll → try .../bin/Debug/net10.0/Foo.dll
        var dir = Path.GetDirectoryName(path);
        if (dir != null && string.Equals(Path.GetFileName(dir), "ref", StringComparison.OrdinalIgnoreCase))
        {
            var implPath = Path.Combine(Path.GetDirectoryName(dir)!, Path.GetFileName(path));
            return implPath; // Caller handles missing file via exception
        }
        return path;
    }

    /// <summary>
    /// Collects all partial method definitions (declarations without implementations) from the compilation.
    /// </summary>
    private static IReadOnlyList<IMethodSymbol> GetAllUnimplementedPartialMethods(Compilation compilation)
    {
        var result = new List<IMethodSymbol>();
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var partialDecls = syntaxTree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword)));

            foreach (var decl in partialDecls)
            {
                if (semanticModel.GetDeclaredSymbol(decl) is IMethodSymbol sym && sym.IsPartialDefinition)
                    result.Add(sym);
            }
        }
        return result;
    }

    private static CSharpCompilation BuildExecutionCompilation(
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation)
    {
        var dummySource = BuildDummyImplementation(allPartialMethods);
        var parseOptions = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
            ?? CSharpParseOptions.Default;
        return (CSharpCompilation)compilation
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(dummySource, parseOptions));
    }

    private static string BuildDummyImplementation(IEnumerable<IMethodSymbol> partialMethods)
    {
        var sb = new StringBuilder();

        var grouped = partialMethods.GroupBy(
            m => (Namespace: m.ContainingType.ContainingNamespace?.IsGlobalNamespace == false
                    ? m.ContainingType.ContainingNamespace.ToDisplayString()
                    : null,
                TypeName: m.ContainingType.Name,
                IsStatic: m.ContainingType.IsStatic,
                TypeKind: m.ContainingType.TypeKind));

        foreach (var typeGroup in grouped)
        {
            var namespaceName = typeGroup.Key.Namespace;
            if (namespaceName != null)
                sb.AppendLine($"namespace {namespaceName} {{");

            var typeKeyword = typeGroup.Key.TypeKind switch
            {
                TypeKind.Struct => "struct",
                _ => "class"
            };

            var typeModifiers = typeGroup.Key.IsStatic ? "static partial" : "partial";
            sb.AppendLine($"{typeModifiers} {typeKeyword} {typeGroup.Key.TypeName} {{");

            foreach (var partialMethod in typeGroup)
            {
                var accessibility = partialMethod.DeclaredAccessibility switch
                {
                    Accessibility.Public => "public",
                    Accessibility.Protected => "protected",
                    Accessibility.Internal => "internal",
                    Accessibility.ProtectedOrInternal => "protected internal",
                    Accessibility.ProtectedAndInternal => "private protected",
                    _ => ""
                };

                var staticModifier = partialMethod.IsStatic ? "static " : "";
                var returnType = partialMethod.ReturnType.ToDisplayString();
                var parameters = string.Join(", ", partialMethod.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));

                sb.AppendLine($"{accessibility} {staticModifier}partial {returnType} {partialMethod.Name}({parameters}) {{");
                if (!partialMethod.ReturnsVoid)
                    sb.AppendLine("return default!;");
                sb.AppendLine("}");
            }

            sb.AppendLine("}");

            if (namespaceName != null)
                sb.AppendLine("}");
        }

        return sb.ToString();
    }
}