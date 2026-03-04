using EasySourceGenerators.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace EasySourceGenerators.Tests;

[TestFixture]
public class DefaultCaseConstValueFluent
{
    [TestCase(0, 888)]
    [TestCase(888, 888)]
    [TestCase(123456789, 888)]
    public void PiExampleLikeGenerator_ProducesExpectedRuntimeOutput(int decimalNumber, int expectedDigit)
    {
        int result = DefaultCaseConstValueFluentClass.Foo(decimalNumber);

        Assert.That(result, Is.EqualTo(expectedDigit));
    }

    [Test]
    public void PiExampleLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DefaultCaseConstValueFluentClass_Foo.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              static partial class DefaultCaseConstValueFluentClass
                              {
                                  public static partial int Foo(int decimalNumber)
                                  {
                                      switch (decimalNumber)
                                      {
                                          default: return 888;
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class DefaultCaseConstValueFluentClass
{
    public static partial int Foo(int decimalNumber);

    [GeneratesMethod(nameof(Foo))]
    static IMethodImplementationGenerator Foo_Generator_Default() =>
        Generate
            .Method().WithParameter<int>().WithReturnType<int>()
            .WithSwitchBody()
            .ForDefaultCase().ReturnConstantValue(_ => 888);
}
