using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

            context.ReportDiagnostic(Diagnostic.Create(
                GeneratingMethodInfo,
                generatorMethod.GetLocation(),
                targetMethodName,
                containingType.Name,
                generatorMethod.Identifier.Text));

            var (returnValue, error) = ExecuteGeneratorMethod(methodSymbol, partialMethodSymbol, compilation);

            if (error != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GeneratorMethodExecutionError,
                    generatorMethod.GetLocation(),
                    methodSymbol.Name,
                    error));
                continue;
            }

            var source = GeneratePartialMethodImplementation(containingType, partialMethodSymbol, returnValue, partialMethodSymbol.ReturnType);
            context.AddSource($"{containingType.Name}_{targetMethodName}.g.cs", source);
        }
    }

    private static (string? value, string? error) ExecuteGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        // Add a dummy implementation for the partial method so the compilation emits cleanly
        var dummySource = BuildDummyImplementation(partialMethod);
        var parseOptions = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
            ?? CSharpParseOptions.Default;
        var dllCompilation = compilation
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(dummySource, parseOptions));

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

            // Resolve referenced assemblies from the compilation's metadata references
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

            var result = method.Invoke(null, null);
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

    private static string BuildDummyImplementation(IMethodSymbol partialMethod)
    {
        var containingType = partialMethod.ContainingType;
        var sb = new StringBuilder();

        var namespaceName = containingType.ContainingNamespace?.IsGlobalNamespace == false
            ? containingType.ContainingNamespace.ToDisplayString()
            : null;

        if (namespaceName != null)
            sb.AppendLine($"namespace {namespaceName} {{");

        var typeKeyword = containingType.TypeKind switch
        {
            TypeKind.Struct => "struct",
            _ => "class"
        };

        sb.AppendLine($"partial {typeKeyword} {containingType.Name} {{");

        var accessibility = partialMethod.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => ""
        };

        var returnType = partialMethod.ReturnType.ToDisplayString();
        var parameters = string.Join(", ", partialMethod.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));

        sb.AppendLine($"{accessibility} partial {returnType} {partialMethod.Name}({parameters}) {{");
        if (!partialMethod.ReturnsVoid)
            sb.AppendLine($"return default!;");
        sb.AppendLine("}");

        sb.AppendLine("}");

        if (namespaceName != null)
            sb.AppendLine("}");

        return sb.ToString();
    }

    private static string FormatReturnLiteral(string? value, ITypeSymbol returnType)
    {
        if (value == null)
            return "null";

        return returnType.SpecialType switch
        {
            SpecialType.System_String => SyntaxFactory.Literal(value).Text,
            SpecialType.System_Char when value.Length == 1 => SyntaxFactory.Literal(value[0]).Text,
            SpecialType.System_Boolean => value.ToLowerInvariant(),
            _ => value
        };
    }

    private static string GeneratePartialMethodImplementation(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        string? returnValue,
        ITypeSymbol returnType)
    {
        var sb = new StringBuilder();

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

        sb.AppendLine($"partial {typeKeyword} {containingType.Name}");
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

        sb.AppendLine($"    {accessibility} partial {returnTypeName} {methodName}({parameters})");
        sb.AppendLine("    {");

        if (!partialMethod.ReturnsVoid)
        {
            var literal = FormatReturnLiteral(returnValue, returnType);
            sb.AppendLine($"        return {literal};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}