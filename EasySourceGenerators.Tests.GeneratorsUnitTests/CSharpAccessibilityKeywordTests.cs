using EasySourceGenerators.Generators.SourceEmitting;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Tests.Generators;

[TestFixture]
public class CSharpAccessibilityKeywordTests
{
    // -----------------------------------------------------------------------
    // ToKeyword with defaultToPrivate = true (default)
    // -----------------------------------------------------------------------

    [Test]
    public void ToKeyword_Public_ReturnsPublic()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Public);

        Assert.That(result, Is.EqualTo("public"));
    }

    [Test]
    public void ToKeyword_Protected_ReturnsProtected()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Protected);

        Assert.That(result, Is.EqualTo("protected"));
    }

    [Test]
    public void ToKeyword_Internal_ReturnsInternal()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Internal);

        Assert.That(result, Is.EqualTo("internal"));
    }

    [Test]
    public void ToKeyword_ProtectedOrInternal_ReturnsProtectedInternal()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.ProtectedOrInternal);

        Assert.That(result, Is.EqualTo("protected internal"));
    }

    [Test]
    public void ToKeyword_ProtectedAndInternal_ReturnsPrivateProtected()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.ProtectedAndInternal);

        Assert.That(result, Is.EqualTo("private protected"));
    }

    [Test]
    public void ToKeyword_Private_ReturnsPrivate()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Private);

        Assert.That(result, Is.EqualTo("private"));
    }

    [Test]
    public void ToKeyword_NotApplicable_ReturnsPrivate()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.NotApplicable);

        Assert.That(result, Is.EqualTo("private"));
    }

    // -----------------------------------------------------------------------
    // ToKeyword with defaultToPrivate = false
    // -----------------------------------------------------------------------

    [Test]
    public void ToKeyword_DefaultToPrivateFalse_Public_ReturnsPublic()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Public, defaultToPrivate: false);

        Assert.That(result, Is.EqualTo("public"));
    }

    [Test]
    public void ToKeyword_DefaultToPrivateFalse_Protected_ReturnsProtected()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Protected, defaultToPrivate: false);

        Assert.That(result, Is.EqualTo("protected"));
    }

    [Test]
    public void ToKeyword_DefaultToPrivateFalse_Internal_ReturnsInternal()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Internal, defaultToPrivate: false);

        Assert.That(result, Is.EqualTo("internal"));
    }

    [Test]
    public void ToKeyword_DefaultToPrivateFalse_Private_ReturnsEmptyString()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.Private, defaultToPrivate: false);

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void ToKeyword_DefaultToPrivateFalse_NotApplicable_ReturnsEmptyString()
    {
        string result = CSharpAccessibilityKeyword.ToKeyword(Accessibility.NotApplicable, defaultToPrivate: false);

        Assert.That(result, Is.EqualTo(""));
    }
}
