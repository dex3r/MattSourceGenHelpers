using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using EasySourceGenerators.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

[Generator]
public sealed class GeneratesMethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<MethodDeclarationSyntax?>> methodsWithAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsMethodWithGeneratesMethodAttribute,
                transform: GetMethodDeclaration)
            .Where(method => method != null)
            .Collect();

        context.RegisterSourceOutput(
            methodsWithAttribute.Combine(context.CompilationProvider),
            (productionContext, data) => GeneratesMethodGenerationPipeline.Execute(productionContext, data.Left, data.Right));
    }

    private static bool IsMethodWithGeneratesMethodAttribute(SyntaxNode node, CancellationToken _)
    {
        if (node is not MethodDeclarationSyntax method)
        {
            return false;
        }
        
        return method.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute => attribute.Name.ToString() is nameof(MethodBodyGenerator));
    }

    private static MethodDeclarationSyntax? GetMethodDeclaration(GeneratorSyntaxContext context, CancellationToken _)
    {
        return context.Node as MethodDeclarationSyntax;
    }
}
