using EasySourceGenerators.Generators.SourceEmitting;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class CSharpTypeKeywordTests
{
    [Test]
    public void From_Class_ReturnsClass()
    {
        string result = CSharpTypeKeyword.From(TypeKind.Class);

        Assert.That(result, Is.EqualTo("class"));
    }

    [Test]
    public void From_Struct_ReturnsStruct()
    {
        string result = CSharpTypeKeyword.From(TypeKind.Struct);

        Assert.That(result, Is.EqualTo("struct"));
    }

    [Test]
    public void From_Interface_ReturnsInterface()
    {
        string result = CSharpTypeKeyword.From(TypeKind.Interface);

        Assert.That(result, Is.EqualTo("interface"));
    }

    [Test]
    public void From_Enum_ReturnsClass()
    {
        string result = CSharpTypeKeyword.From(TypeKind.Enum);

        Assert.That(result, Is.EqualTo("class"));
    }

    [Test]
    public void From_Delegate_ReturnsClass()
    {
        string result = CSharpTypeKeyword.From(TypeKind.Delegate);

        Assert.That(result, Is.EqualTo("class"));
    }
}
