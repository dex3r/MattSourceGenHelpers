using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators.SourceEmitting;

/// <summary>
/// Maps Roslyn <see cref="Accessibility"/> values to their C# keyword representations.
/// </summary>
internal static class CSharpAccessibilityKeyword
{
    /// <summary>
    /// Converts a Roslyn <see cref="Accessibility"/> value to its C# keyword representation.
    /// When <paramref name="defaultToPrivate"/> is <c>true</c> (the default), returns <c>"private"</c>
    /// for <see cref="Accessibility.Private"/>, <see cref="Accessibility.NotApplicable"/>,
    /// and any unrecognized values.
    /// When <c>false</c>, returns an empty string instead — useful in contexts where
    /// the <c>private</c> keyword is implicit (e.g., dummy implementations).
    /// </summary>
    internal static string ToKeyword(Accessibility accessibility, bool defaultToPrivate = true)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => defaultToPrivate ? "private" : ""
        };
    }
}
