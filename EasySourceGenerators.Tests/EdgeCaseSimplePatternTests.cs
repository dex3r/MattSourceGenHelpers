using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Tests;

[TestFixture]
public class EdgeCaseSimplePatternTests
{
    // -----------------------------------------------------------------------
    // Void return type
    // -----------------------------------------------------------------------

    [Test]
    public void VoidReturnGenerator_MethodIsCallable()
    {
        TestVoidClass instance = new TestVoidClass();
        Assert.DoesNotThrow(() => instance.DoSomething());
    }

    [Test]
    public void VoidReturnGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestVoidClass_DoSomething.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              partial class TestVoidClass
                              {
                                  public partial void DoSomething()
                                  {
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }

    // -----------------------------------------------------------------------
    // Bool return type
    // -----------------------------------------------------------------------

    [Test]
    public void BoolReturnGenerator_ProducesExpectedRuntimeOutput()
    {
        bool result = TestBoolReturnClass.IsEnabled();
        Assert.That(result, Is.True);
    }

    [Test]
    public void BoolReturnGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestBoolReturnClass_IsEnabled.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              static partial class TestBoolReturnClass
                              {
                                  public static partial bool IsEnabled()
                                  {
                                      return true;
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }

    // -----------------------------------------------------------------------
    // Char return type
    // -----------------------------------------------------------------------

    [Test]
    public void CharReturnGenerator_ProducesExpectedRuntimeOutput()
    {
        char result = TestCharReturnClass.GetSymbol();
        Assert.That(result, Is.EqualTo('A'));
    }

    [Test]
    public void CharReturnGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestCharReturnClass_GetSymbol.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              static partial class TestCharReturnClass
                              {
                                  public static partial char GetSymbol()
                                  {
                                      return 'A';
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }

    // -----------------------------------------------------------------------
    // Internal method accessibility
    // -----------------------------------------------------------------------

    [Test]
    public void InternalMethodGenerator_ProducesExpectedRuntimeOutput()
    {
        TestInternalMethodClass instance = new TestInternalMethodClass();
        string result = instance.GetValue();
        Assert.That(result, Is.EqualTo("internal_value"));
    }

    [Test]
    public void InternalMethodGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestInternalMethodClass_GetValue.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              partial class TestInternalMethodClass
                              {
                                  internal partial string GetValue()
                                  {
                                      return "internal_value";
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public partial class TestVoidClass
{
    public partial void DoSomething();

    [MethodBodyGenerator(nameof(DoSomething))]
    private static void DoSomething_Generator() { }
}

public static partial class TestBoolReturnClass
{
    public static partial bool IsEnabled();

    [MethodBodyGenerator(nameof(IsEnabled))]
    private static bool IsEnabled_Generator() => true;
}

public static partial class TestCharReturnClass
{
    public static partial char GetSymbol();

    [MethodBodyGenerator(nameof(GetSymbol))]
    private static char GetSymbol_Generator() => 'A';
}

public partial class TestInternalMethodClass
{
    internal partial string GetValue();

    [MethodBodyGenerator(nameof(GetValue))]
    private static string GetValue_Generator() => "internal_value";
}
