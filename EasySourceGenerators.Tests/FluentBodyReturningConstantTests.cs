using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Tests;

[TestFixture]
public class FluentBodyReturningConstantTests
{
    [Test]
    public void FluentBodyReturningConstant_ProducesExpectedRuntimeOutput()
    {
        TestFluentConstantClass instance = new TestFluentConstantClass();

        string result = instance.GetGreeting();

        Assert.That(result, Is.EqualTo("Hello from fluent API"));
    }

    [Test]
    public void FluentBodyReturningConstant_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestFluentConstantClass_GetGreeting.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              partial class TestFluentConstantClass
                              {
                                  public partial string GetGreeting()
                                  {
                                      return "Hello from fluent API";
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }

    [Test]
    public void FluentBodyReturningConstant_Int_ProducesExpectedRuntimeOutput()
    {
        int result = TestFluentConstantIntClass.GetMagicNumber();

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void FluentBodyReturningConstant_Int_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestFluentConstantIntClass_GetMagicNumber.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              static partial class TestFluentConstantIntClass
                              {
                                  public static partial int GetMagicNumber()
                                  {
                                      return 42;
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public partial class TestFluentConstantClass
{
    public partial string GetGreeting();

    [MethodBodyGenerator(nameof(GetGreeting))]
    static IMethodBodyGenerator GetGreeting_Generator() =>
        Generate
            .MethodBody().ForMethod().WithReturnType<string>().WithNoParameters()
            .BodyReturningConstant(() => "Hello from fluent API");
}

public static partial class TestFluentConstantIntClass
{
    public static partial int GetMagicNumber();

    [MethodBodyGenerator(nameof(GetMagicNumber))]
    static IMethodBodyGenerator GetMagicNumber_Generator() =>
        Generate
            .MethodBody().ForMethod().WithReturnType<int>().WithNoParameters()
            .BodyReturningConstant(() => 42);
}
