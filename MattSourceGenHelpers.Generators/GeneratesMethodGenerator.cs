using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
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

            var source = GeneratePartialMethodImplementation(containingType, partialMethodSymbol, methodSymbol);
            context.AddSource($"{containingType.Name}_{targetMethodName}.g.cs", source);
        }
    }

    private static string GeneratePartialMethodImplementation(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        IMethodSymbol generatorMethod)
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

        var returnType = partialMethod.ReturnType.ToDisplayString();
        var methodName = partialMethod.Name;
        var parameters = string.Join(", ", partialMethod.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));

        sb.AppendLine($"    {accessibility} partial {returnType} {methodName}({parameters})");
        sb.AppendLine("    {");

        if (partialMethod.ReturnsVoid)
        {
            sb.AppendLine($"        {generatorMethod.Name}();");
        }
        else
        {
            sb.AppendLine($"        return {generatorMethod.Name}();");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}