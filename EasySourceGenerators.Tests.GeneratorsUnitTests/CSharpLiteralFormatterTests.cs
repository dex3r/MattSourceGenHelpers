using EasySourceGenerators.Generators.SourceEmitting;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Tests.Generators;

[TestFixture]
public class CSharpLiteralFormatterTests
{
    // -----------------------------------------------------------------------
    // FormatValueAsLiteral
    // -----------------------------------------------------------------------

    [Test]
    public void FormatValueAsLiteral_NullValue_ReturnsDefault()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral(null, SpecialType.System_String, TypeKind.Class, "string");

        Assert.That(result, Is.EqualTo("default"));
    }

    [Test]
    public void FormatValueAsLiteral_StringType_ReturnsQuotedLiteral()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("hello", SpecialType.System_String, TypeKind.Class, "string");

        Assert.That(result, Is.EqualTo("\"hello\""));
    }

    [Test]
    public void FormatValueAsLiteral_StringWithSpecialCharacters_ReturnsEscapedLiteral()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("line1\nline2", SpecialType.System_String, TypeKind.Class, "string");

        Assert.That(result, Does.Contain("\\n"));
    }

    [Test]
    public void FormatValueAsLiteral_CharType_ReturnsSingleQuotedLiteral()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("A", SpecialType.System_Char, TypeKind.Struct, "char");

        Assert.That(result, Is.EqualTo("'A'"));
    }

    [Test]
    public void FormatValueAsLiteral_BooleanTrue_ReturnsLowercase()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("True", SpecialType.System_Boolean, TypeKind.Struct, "bool");

        Assert.That(result, Is.EqualTo("true"));
    }

    [Test]
    public void FormatValueAsLiteral_BooleanFalse_ReturnsLowercase()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("False", SpecialType.System_Boolean, TypeKind.Struct, "bool");

        Assert.That(result, Is.EqualTo("false"));
    }

    [Test]
    public void FormatValueAsLiteral_EnumType_ReturnsPrefixedValue()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("Red", SpecialType.None, TypeKind.Enum, "MyNamespace.Colors");

        Assert.That(result, Is.EqualTo("MyNamespace.Colors.Red"));
    }

    [Test]
    public void FormatValueAsLiteral_IntegerType_ReturnsValueAsIs()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("42", SpecialType.System_Int32, TypeKind.Struct, "int");

        Assert.That(result, Is.EqualTo("42"));
    }

    [Test]
    public void FormatValueAsLiteral_DecimalType_ReturnsValueAsIs()
    {
        string result = CSharpLiteralFormatter.FormatValueAsLiteral("3.14", SpecialType.System_Double, TypeKind.Struct, "double");

        Assert.That(result, Is.EqualTo("3.14"));
    }

    // -----------------------------------------------------------------------
    // FormatKeyAsLiteral
    // -----------------------------------------------------------------------

    [Test]
    public void FormatKeyAsLiteral_EnumType_ReturnsPrefixedValue()
    {
        string result = CSharpLiteralFormatter.FormatKeyAsLiteral("Green", TypeKind.Enum, "MyNamespace.Colors");

        Assert.That(result, Is.EqualTo("MyNamespace.Colors.Green"));
    }

    [Test]
    public void FormatKeyAsLiteral_BoolTrue_ReturnsTrueLiteral()
    {
        string result = CSharpLiteralFormatter.FormatKeyAsLiteral(true, TypeKind.Struct, "bool");

        Assert.That(result, Is.EqualTo("true"));
    }

    [Test]
    public void FormatKeyAsLiteral_BoolFalse_ReturnsFalseLiteral()
    {
        string result = CSharpLiteralFormatter.FormatKeyAsLiteral(false, TypeKind.Struct, "bool");

        Assert.That(result, Is.EqualTo("false"));
    }

    [Test]
    public void FormatKeyAsLiteral_String_ReturnsQuotedLiteral()
    {
        string result = CSharpLiteralFormatter.FormatKeyAsLiteral("hello", TypeKind.Class, "string");

        Assert.That(result, Is.EqualTo("\"hello\""));
    }

    [Test]
    public void FormatKeyAsLiteral_Integer_ReturnsToStringResult()
    {
        string result = CSharpLiteralFormatter.FormatKeyAsLiteral(42, TypeKind.Struct, "int");

        Assert.That(result, Is.EqualTo("42"));
    }

    [Test]
    public void FormatKeyAsLiteral_NullTypeKind_UsesDefaultBehavior()
    {
        string result = CSharpLiteralFormatter.FormatKeyAsLiteral(99, null, null);

        Assert.That(result, Is.EqualTo("99"));
    }
}
