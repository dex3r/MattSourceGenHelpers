using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Extracts the delegate body source code from the outermost invocation's lambda argument
/// in a generator method's return expression. The extracted body is re-indented to match
/// the target method body indentation (8 spaces).
/// </summary>
internal static class DelegateBodySyntaxExtractor
{
    private const string MethodBodyIndent = "        ";

    /// <summary>
    /// Attempts to find the lambda argument of the outermost invocation in the generator
    /// method's return expression and extract the lambda body. Returns <c>null</c> if no
    /// such lambda is found.
    /// For expression lambdas, returns the expression text.
    /// For block lambdas, returns the block body re-indented to the method body level.
    /// </summary>
    internal static string? TryExtractDelegateBody(MethodDeclarationSyntax generatorMethodSyntax)
    {
        ExpressionSyntax? returnExpression = GetReturnExpression(generatorMethodSyntax);
        if (returnExpression is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        ArgumentSyntax? argument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (argument?.Expression is not LambdaExpressionSyntax lambda)
        {
            return null;
        }

        if (lambda.Body is ExpressionSyntax expression)
        {
            string expressionText = expression.ToFullString().Trim();
            return expressionText;
        }

        if (lambda.Body is BlockSyntax block)
        {
            return ExtractBlockBody(block);
        }

        return null;
    }

    /// <summary>
    /// Gets the return expression from a generator method. Handles both expression-body
    /// methods (<c>=&gt; expr</c>) and block-body methods (<c>{ return expr; }</c>).
    /// </summary>
    private static ExpressionSyntax? GetReturnExpression(MethodDeclarationSyntax method)
    {
        if (method.ExpressionBody != null)
        {
            return method.ExpressionBody.Expression;
        }

        if (method.Body != null)
        {
            ReturnStatementSyntax? returnStatement = method.Body.Statements
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault();
            return returnStatement?.Expression;
        }

        return null;
    }

    /// <summary>
    /// Extracts the content of a block body (between <c>{</c> and <c>}</c>),
    /// determines the base indentation, and re-indents all lines to the method body level.
    /// Blank lines between statements are preserved with method body indentation.
    /// </summary>
    private static string? ExtractBlockBody(BlockSyntax block)
    {
        string blockText = block.ToFullString();
        string[] lines = blockText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        int openIndex = -1;
        int closeIndex = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            if (openIndex == -1 && lines[i].TrimEnd().EndsWith("{", StringComparison.Ordinal))
            {
                openIndex = i;
                break;
            }
        }

        for (int i = lines.Length - 1; i >= 0; i--)
        {
            string trimmed = lines[i].Trim();
            if (trimmed.StartsWith("}", StringComparison.Ordinal))
            {
                closeIndex = i;
                break;
            }
        }

        if (openIndex == -1 || closeIndex == -1 || closeIndex <= openIndex)
        {
            return null;
        }

        string[] contentLines = new string[closeIndex - openIndex - 1];
        Array.Copy(lines, openIndex + 1, contentLines, 0, contentLines.Length);

        if (contentLines.Length == 0)
        {
            return null;
        }

        int minIndent = int.MaxValue;
        foreach (string line in contentLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            int indent = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                {
                    indent++;
                }
                else if (c == '\t')
                {
                    indent += 4;
                }
                else
                {
                    break;
                }
            }

            if (indent < minIndent)
            {
                minIndent = indent;
            }
        }

        if (minIndent == int.MaxValue)
        {
            minIndent = 0;
        }

        StringBuilder result = new();
        for (int i = 0; i < contentLines.Length; i++)
        {
            string line = contentLines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                result.AppendLine(MethodBodyIndent);
            }
            else
            {
                string stripped = minIndent <= line.Length ? line.Substring(minIndent) : line.TrimStart();
                string trimmedEnd = stripped.TrimEnd();
                result.AppendLine(MethodBodyIndent + trimmedEnd);
            }
        }

        return result.ToString().TrimEnd('\n', '\r');
    }
}
