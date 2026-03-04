using EasySourceGenerators.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace EasySourceGenerators.Tests;

[TestFixture]
public class DefaultCaseConstValue
{
    [TestCase(0, 777)]
    [TestCase(777, 777)]
    [TestCase(123456789, 777)]
    public void PiExampleLikeGenerator_ProducesExpectedRuntimeOutput(int decimalNumber, int expectedDigit)
    {
        int result = DefaultCaseConstValueClass.Foo(decimalNumber);

        Assert.That(result, Is.EqualTo(expectedDigit));
    }

    [Test]
    public void PiExampleLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DefaultCaseConstValueClass_Foo.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              static partial class DefaultCaseConstValueClass
                              {
                                  public static partial int Foo(int decimalNumber)
                                  {
                                      switch (decimalNumber)
                                      {
                                          default: return 777;
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class DefaultCaseConstValueClass
{
    public static partial int Foo(int decimalNumber);

    [GeneratesMethod(nameof(Foo))]
    [SwitchDefault]
    static Func<int, int> Foo_Generator_Default() => decimalNumber => 777;
}
