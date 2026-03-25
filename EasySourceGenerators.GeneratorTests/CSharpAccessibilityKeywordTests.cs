using EasySourceGenerators.Generators.SourceEmitting;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class CSharpAccessibilityKeywordTests
{
    // -----------------------------------------------------------------------
    // From (returns "private" as default)
    // -----------------------------------------------------------------------

    [Test]
    public void From_Public_ReturnsPublic()
    {
        string result = CSharpAccessibilityKeyword.From(Accessibility.Public);

        Assert.That(result, Is.EqualTo("public"));
    }

    [Test]
    public void From_Protected_ReturnsProtected()
    {
        string result = CSharpAccessibilityKeyword.From(Accessibility.Protected);

        Assert.That(result, Is.EqualTo("protected"));
    }

    [Test]
    public void From_Internal_ReturnsInternal()
    {
        string result = CSharpAccessibilityKeyword.From(Accessibility.Internal);

        Assert.That(result, Is.EqualTo("internal"));
    }

    [Test]
    public void From_ProtectedOrInternal_ReturnsProtectedInternal()
    {
        string result = CSharpAccessibilityKeyword.From(Accessibility.ProtectedOrInternal);

        Assert.That(result, Is.EqualTo("protected internal"));
    }

    [Test]
    public void From_ProtectedAndInternal_ReturnsPrivateProtected()
    {
        string result = CSharpAccessibilityKeyword.From(Accessibility.ProtectedAndInternal);

        Assert.That(result, Is.EqualTo("private protected"));
    }

    [Test]
    public void From_Private_ReturnsPrivate()
    {
        string result = CSharpAccessibilityKeyword.From(Accessibility.Private);

        Assert.That(result, Is.EqualTo("private"));
    }

    [Test]
    public void From_NotApplicable_ReturnsPrivate()
    {
        string result = CSharpAccessibilityKeyword.From(Accessibility.NotApplicable);

        Assert.That(result, Is.EqualTo("private"));
    }

    // -----------------------------------------------------------------------
    // FromOrEmpty (returns "" as default)
    // -----------------------------------------------------------------------

    [Test]
    public void FromOrEmpty_Public_ReturnsPublic()
    {
        string result = CSharpAccessibilityKeyword.FromOrEmpty(Accessibility.Public);

        Assert.That(result, Is.EqualTo("public"));
    }

    [Test]
    public void FromOrEmpty_Protected_ReturnsProtected()
    {
        string result = CSharpAccessibilityKeyword.FromOrEmpty(Accessibility.Protected);

        Assert.That(result, Is.EqualTo("protected"));
    }

    [Test]
    public void FromOrEmpty_Internal_ReturnsInternal()
    {
        string result = CSharpAccessibilityKeyword.FromOrEmpty(Accessibility.Internal);

        Assert.That(result, Is.EqualTo("internal"));
    }

    [Test]
    public void FromOrEmpty_Private_ReturnsEmptyString()
    {
        string result = CSharpAccessibilityKeyword.FromOrEmpty(Accessibility.Private);

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void FromOrEmpty_NotApplicable_ReturnsEmptyString()
    {
        string result = CSharpAccessibilityKeyword.FromOrEmpty(Accessibility.NotApplicable);

        Assert.That(result, Is.EqualTo(""));
    }
}
