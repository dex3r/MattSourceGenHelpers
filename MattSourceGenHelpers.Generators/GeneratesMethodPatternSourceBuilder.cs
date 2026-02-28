using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace MattSourceGenHelpers.Generators;

internal static class GeneratesMethodPatternSourceBuilder
{
    private const string SwitchCaseAttributeTypeName = "MattSourceGenHelpers.Abstractions.SwitchCase";
    private const string SwitchDefaultAttributeTypeName = "MattSourceGenHelpers.Abstractions.SwitchDefault";

    internal static string GenerateFromSwitchAttributes(
        SourceProductionContext context,
        List<GeneratesMethodGenerationTarget> methods,
        IMethodSymbol partialMethod,
        INamedTypeSymbol containingType,
        IReadOnlyList<IMethodSymbol> allPartials,
        Compilation compilation)
    {
        List<GeneratesMethodGenerationTarget> switchCaseMethods = methods
            .Where(method => method.Symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == SwitchCaseAttributeTypeName))
            .ToList();
        GeneratesMethodGenerationTarget? switchDefaultMethod = methods
            .FirstOrDefault(method => method.Symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == SwitchDefaultAttributeTypeName));

        List<(object key, string value)> switchCases = new();
        foreach (GeneratesMethodGenerationTarget switchMethod in switchCaseMethods)
        {
            IEnumerable<AttributeData> switchCaseAttributes = switchMethod.Symbol.GetAttributes()
                .Where(attribute => attribute.AttributeClass?.ToDisplayString() == SwitchCaseAttributeTypeName);

            foreach (AttributeData switchCaseAttribute in switchCaseAttributes)
            {
                if (switchCaseAttribute.ConstructorArguments.Length == 0)
                {
                    continue;
                }

                object? caseArgument = switchCaseAttribute.ConstructorArguments[0].Value;
                if (caseArgument is null)
                {
                    continue;
                }

                (string? result, string? error) = GeneratesMethodExecutionRuntime.ExecuteGeneratorMethodWithArgs(
                    switchMethod.Symbol,
                    allPartials,
                    compilation,
                    new[] { caseArgument });

                if (error != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GeneratesMethodGeneratorDiagnostics.GeneratorMethodExecutionError,
                        switchMethod.Syntax.GetLocation(),
                        switchMethod.Symbol.Name,
                        error));
                    continue;
                }

                switchCases.Add((caseArgument, FormatValueAsCSharpLiteral(result, partialMethod.ReturnType)));
            }
        }

        string? defaultExpression = switchDefaultMethod is not null
            ? ExtractDefaultExpressionFromSwitchDefaultMethod(switchDefaultMethod.Syntax)
            : null;

        return GenerateSwitchMethodSource(containingType, partialMethod, switchCases, defaultExpression);
    }

    internal static string GenerateFromFluent(
        SourceProductionContext context,
        GeneratesMethodGenerationTarget methodInfo,
        IMethodSymbol partialMethod,
        INamedTypeSymbol containingType,
        Compilation compilation)
    {
        (SwitchBodyData? record, string? error) = GeneratesMethodExecutionRuntime.ExecuteFluentGeneratorMethod(
            methodInfo.Symbol,
            partialMethod,
            compilation);

        if (error != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GeneratesMethodGeneratorDiagnostics.GeneratorMethodExecutionError,
                methodInfo.Syntax.GetLocation(),
                methodInfo.Symbol.Name,
                error));
            return string.Empty;
        }

        SwitchBodyData switchBodyData = record!;
        string? defaultExpression = switchBodyData.HasDefaultCase
            ? ExtractDefaultExpressionFromFluentMethod(methodInfo.Syntax)
            : null;

        return GenerateSwitchMethodSource(containingType, partialMethod, switchBodyData.CasePairs, defaultExpression);
    }

    internal static string GenerateSimplePartialMethod(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        string? returnValue)
    {
        StringBuilder builder = new();
        AppendNamespaceAndTypeHeader(builder, containingType, partialMethod);

        if (!partialMethod.ReturnsVoid)
        {
            string literal = FormatValueAsCSharpLiteral(returnValue, partialMethod.ReturnType);
            builder.AppendLine($"        return {literal};");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string? ExtractDefaultExpressionFromSwitchDefaultMethod(MethodDeclarationSyntax method)
    {
        ExpressionSyntax? bodyExpression = method.ExpressionBody?.Expression;
        if (bodyExpression == null && method.Body != null)
        {
            ReturnStatementSyntax? returnStatement = method.Body.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
            bodyExpression = returnStatement?.Expression;
        }

        return ExtractInnermostLambdaBody(bodyExpression);
    }

    private static string? ExtractDefaultExpressionFromFluentMethod(MethodDeclarationSyntax method)
    {
        IEnumerable<InvocationExpressionSyntax> invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
            {
                continue;
            }

            string methodName = memberAccessExpression.Name.Identifier.Text;
            if (methodName is not ("ReturnConstantValue" or "UseBody"))
            {
                continue;
            }

            ExpressionSyntax? argumentExpression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            return ExtractInnermostLambdaBody(argumentExpression);
        }

        return null;
    }

    private static string? ExtractInnermostLambdaBody(ExpressionSyntax? expression)
    {
        while (true)
        {
            switch (expression)
            {
                case SimpleLambdaExpressionSyntax simpleLambdaExpression:
                    expression = simpleLambdaExpression.Body as ExpressionSyntax;
                    break;
                case ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression:
                    expression = parenthesizedLambdaExpression.Body as ExpressionSyntax;
                    break;
                default:
                    return expression?.ToString();
            }
        }
    }

    private static string GenerateSwitchMethodSource(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        IReadOnlyList<(object key, string value)> cases,
        string? defaultExpression)
    {
        StringBuilder builder = new();
        AppendNamespaceAndTypeHeader(builder, containingType, partialMethod);

        if (partialMethod.Parameters.Length == 0)
        {
            string fallbackExpression = defaultExpression ?? "default";
            builder.AppendLine($"        return {fallbackExpression};");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        string switchParameterName = partialMethod.Parameters[0].Name;
        builder.AppendLine($"        switch ({switchParameterName})");
        builder.AppendLine("        {");

        foreach ((object key, string value) in cases)
        {
            builder.AppendLine($"            case {key}: return {value};");
        }

        if (defaultExpression != null)
        {
            builder.AppendLine($"            default: return {defaultExpression};");
        }

        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void AppendNamespaceAndTypeHeader(StringBuilder builder, INamedTypeSymbol containingType, IMethodSymbol partialMethod)
    {
        string? namespaceName = containingType.ContainingNamespace?.IsGlobalNamespace == false
            ? containingType.ContainingNamespace.ToDisplayString()
            : null;
        if (namespaceName != null)
        {
            builder.AppendLine($"namespace {namespaceName};");
            builder.AppendLine();
        }

        string typeKeyword = containingType.TypeKind switch
        {
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            _ => "class"
        };

        string typeModifiers = containingType.IsStatic ? "static partial" : "partial";
        builder.AppendLine($"{typeModifiers} {typeKeyword} {containingType.Name}");
        builder.AppendLine("{");

        string accessibility = partialMethod.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private"
        };

        string returnTypeName = partialMethod.ReturnType.ToDisplayString();
        string methodName = partialMethod.Name;
        string parameters = string.Join(", ", partialMethod.Parameters.Select(parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}"));
        string methodModifiers = partialMethod.IsStatic ? "static partial" : "partial";

        builder.AppendLine($"    {accessibility} {methodModifiers} {returnTypeName} {methodName}({parameters})");
        builder.AppendLine("    {");
    }

    internal static string FormatValueAsCSharpLiteral(string? value, ITypeSymbol returnType)
    {
        if (value == null)
        {
            return "default";
        }

        return returnType.SpecialType switch
        {
            SpecialType.System_String => SyntaxFactory.Literal(value).Text,
            SpecialType.System_Char when value.Length == 1 => SyntaxFactory.Literal(value[0]).Text,
            SpecialType.System_Boolean => value.ToLowerInvariant(),
            _ => value
        };
    }
}
