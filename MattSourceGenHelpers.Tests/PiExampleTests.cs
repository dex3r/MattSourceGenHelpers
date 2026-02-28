using MattSourceGenHelpers.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace MattSourceGenHelpers.Tests;

[TestFixture]
public class PiExampleTests
{
    [Test]
    public void SwitchCaseAttribute_StoresArg1Value()
    {
        SwitchCase switchCase = new(arg1: 7);

        Assert.That(switchCase.Arg1, Is.EqualTo(7));
    }

    [TestCase(0, 3)]
    [TestCase(1, 1)]
    [TestCase(2, 4)]
    [TestCase(5, 9)]
    public void PiExampleLikeGenerator_ProducesExpectedRuntimeOutput(int decimalNumber, int expectedDigit)
    {
        int result = TestPiClass.GetPiDecimal(decimalNumber);

        Assert.That(result, Is.EqualTo(expectedDigit));
    }

    [Test]
    public void PiExampleLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestPiClass_GetPiDecimal.g.cs");
        string expectedCode = """
                              namespace MattSourceGenHelpers.Tests;

                              static partial class TestPiClass
                              {
                                  public static partial int GetPiDecimal(int decimalNumber)
                                  {
                                      switch (decimalNumber)
                                      {
                                          case 0: return 3;
                                          case 1: return 1;
                                          case 2: return 4;
                                          default: return TestSlowMath.CalculatePiDecimal(decimalNumber);
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class TestPiClass
{
    public static partial int GetPiDecimal(int decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    [SwitchCase(arg1: 0)]
    [SwitchCase(arg1: 1)]
    [SwitchCase(arg1: 2)]
    static int GetPiDecimal_Generator_Specialized(int decimalNumber) =>
        TestSlowMath.CalculatePiDecimal(decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    [SwitchDefault]
    static Func<int, int> GetPiDecimal_Generator_Fallback() => decimalNumber => TestSlowMath.CalculatePiDecimal(decimalNumber);
}
