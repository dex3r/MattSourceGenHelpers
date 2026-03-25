using System.Collections.Generic;
using System.Text;

namespace EasySourceGenerators.Generators.SourceEmitting;

/// <summary>
/// Contains the data for a group of dummy partial method implementations within a single type.
/// </summary>
internal sealed record DummyTypeGroupData(
    string? NamespaceName,
    string TypeName,
    string TypeKeyword,
    string TypeModifiers,
    IReadOnlyList<DummyMethodData> Methods);

/// <summary>
/// Contains the data for a single dummy partial method implementation.
/// </summary>
internal sealed record DummyMethodData(
    string AccessibilityKeyword,
    string MethodModifiers,
    string ReturnTypeName,
    string MethodName,
    string ParameterList,
    string BodyStatement);

/// <summary>
/// Emits dummy partial method implementations used during generator execution compilations.
/// These implementations throw exceptions when called, preventing accidental invocations
/// of unimplemented partial methods during compile-time code generation.
/// </summary>
internal static class DummyImplementationEmitter
{
    /// <summary>
    /// Generates C# source containing dummy implementations for all provided type groups.
    /// Each dummy method body consists of a single statement (typically a throw expression).
    /// </summary>
    internal static string Emit(IEnumerable<DummyTypeGroupData> typeGroups)
    {
        StringBuilder builder = new();

        foreach (DummyTypeGroupData typeGroup in typeGroups)
        {
            if (typeGroup.NamespaceName != null)
            {
                builder.AppendLine($"namespace {typeGroup.NamespaceName} {{");
            }

            builder.AppendLine($"{typeGroup.TypeModifiers} {typeGroup.TypeKeyword} {typeGroup.TypeName} {{");

            foreach (DummyMethodData method in typeGroup.Methods)
            {
                builder.AppendLine($"{method.AccessibilityKeyword} {method.MethodModifiers} {method.ReturnTypeName} {method.MethodName}({method.ParameterList}) {{");
                builder.AppendLine(method.BodyStatement);
                builder.AppendLine("}");
            }

            builder.AppendLine("}");

            if (typeGroup.NamespaceName != null)
            {
                builder.AppendLine("}");
            }
        }

        return builder.ToString();
    }
}
