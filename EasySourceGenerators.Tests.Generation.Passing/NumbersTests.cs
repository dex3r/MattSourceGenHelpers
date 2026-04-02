using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Tests.Generation.Passing.Helpers;

namespace EasySourceGenerators.Tests.Generation.Passing;

public class NumbersTests
{
    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedRuntimeOutput()
    {
        TestNumbersClass testColorsClass = new TestNumbersClass();

        int number = testColorsClass.GetResultNumber();

        Assert.That(number, Is.EqualTo(15));
    }

    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestNumbersClass_GetResultNumber.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests.Generation.Passing;

                              partial class TestNumbersClass
                              {
                                  public partial int GetResultNumber()
                                  {
                                      return 15;
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public partial class TestNumbersClass
{
    public partial int GetResultNumber();

    [MethodBodyGenerator(sameClassMethodName: nameof(GetResultNumber))]
    private static int GetAllColorsString_Generator() => 5 + 10;
}
