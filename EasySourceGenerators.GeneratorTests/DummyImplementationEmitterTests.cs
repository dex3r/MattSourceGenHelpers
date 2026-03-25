using EasySourceGenerators.Generators.SourceEmitting;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class DummyImplementationEmitterTests
{
    [Test]
    public void Emit_SingleMethodWithNamespace_ContainsNamespaceDeclaration()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: "TestNamespace",
                TypeName: "MyClass",
                TypeKeyword: "class",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "",
                        ReturnTypeName: "string",
                        MethodName: "GetValue",
                        ParameterList: "",
                        BodyStatement: "throw new System.Exception();")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Contain("namespace TestNamespace {"));
    }

    [Test]
    public void Emit_SingleMethodWithNamespace_ContainsClosingNamespaceBrace()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: "TestNamespace",
                TypeName: "MyClass",
                TypeKeyword: "class",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "",
                        ReturnTypeName: "string",
                        MethodName: "GetValue",
                        ParameterList: "",
                        BodyStatement: "throw new System.Exception();")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        // Count closing braces: should have namespace }, type }, and method }
        int closingBraceCount = result.Split('\n').Count(line => line.Trim() == "}");
        Assert.That(closingBraceCount, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void Emit_WithoutNamespace_DoesNotContainNamespaceDeclaration()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: null,
                TypeName: "MyClass",
                TypeKeyword: "class",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "",
                        ReturnTypeName: "void",
                        MethodName: "DoWork",
                        ParameterList: "",
                        BodyStatement: "throw new System.Exception();")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Not.Contain("namespace"));
    }

    [Test]
    public void Emit_ContainsTypeDeclaration()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: null,
                TypeName: "Helper",
                TypeKeyword: "class",
                TypeModifiers: "static partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "static ",
                        ReturnTypeName: "int",
                        MethodName: "Calculate",
                        ParameterList: "",
                        BodyStatement: "throw new System.Exception();")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Contain("static partial class Helper {"));
    }

    [Test]
    public void Emit_ContainsMethodWithPartialKeyword()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: null,
                TypeName: "MyClass",
                TypeKeyword: "class",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "",
                        ReturnTypeName: "string",
                        MethodName: "GetValue",
                        ParameterList: "",
                        BodyStatement: "return \"dummy\";")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Contain("public partial string GetValue() {"));
    }

    [Test]
    public void Emit_StaticMethod_ContainsStaticModifier()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: null,
                TypeName: "MyClass",
                TypeKeyword: "class",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "static ",
                        ReturnTypeName: "int",
                        MethodName: "Compute",
                        ParameterList: "int x",
                        BodyStatement: "return 0;")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Contain("public static partial int Compute(int x) {"));
    }

    [Test]
    public void Emit_ContainsBodyStatement()
    {
        string expectedBody = "throw new global::MyException(\"test\");";
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: null,
                TypeName: "MyClass",
                TypeKeyword: "class",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "",
                        ReturnTypeName: "void",
                        MethodName: "DoWork",
                        ParameterList: "",
                        BodyStatement: expectedBody)
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Contain(expectedBody));
    }

    [Test]
    public void Emit_MultipleMethodsInSameType_ContainsBothMethods()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: "NS",
                TypeName: "MyClass",
                TypeKeyword: "class",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "",
                        ReturnTypeName: "string",
                        MethodName: "First",
                        ParameterList: "",
                        BodyStatement: "throw new System.Exception();"),
                    new DummyMethodData(
                        AccessibilityKeyword: "internal",
                        StaticModifier: "",
                        ReturnTypeName: "int",
                        MethodName: "Second",
                        ParameterList: "int x",
                        BodyStatement: "throw new System.Exception();")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Contain("partial string First()"));
        Assert.That(result, Does.Contain("partial int Second(int x)"));
    }

    [Test]
    public void Emit_EmptyGroups_ReturnsEmptyString()
    {
        string result = DummyImplementationEmitter.Emit(new List<DummyTypeGroupData>());

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void Emit_StructType_ContainsStructKeyword()
    {
        List<DummyTypeGroupData> groups = new()
        {
            new DummyTypeGroupData(
                NamespaceName: null,
                TypeName: "MyStruct",
                TypeKeyword: "struct",
                TypeModifiers: "partial",
                Methods: new List<DummyMethodData>
                {
                    new DummyMethodData(
                        AccessibilityKeyword: "public",
                        StaticModifier: "",
                        ReturnTypeName: "int",
                        MethodName: "GetValue",
                        ParameterList: "",
                        BodyStatement: "return 0;")
                })
        };

        string result = DummyImplementationEmitter.Emit(groups);

        Assert.That(result, Does.Contain("partial struct MyStruct {"));
    }
}
