using MattSourceGenHelpers.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace MattSourceGenHelpers.Tests;

[TestFixture]
public class PiExampleFluentTests
{
    [TestCase(0, 3)]
    [TestCase(1, 1)]
    [TestCase(2, 4)]
    [TestCase(300, 3)]
    [TestCase(301, 7)]
    [TestCase(302, 2)]
    [TestCase(303, 4)]
    [TestCase(5, 9)]
    public void PiExampleFluentLikeGenerator_ProducesExpectedRuntimeOutput(int decimalNumber, int expectedDigit)
    {
        int result = TestPiFluentClass.GetPiDecimal(decimalNumber);

        Assert.That(result, Is.EqualTo(expectedDigit));
    }

    [Test]
    public void PiExampleFluentLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestPiFluentClass_GetPiDecimal.g.cs");
        string expectedCode = """
                              namespace MattSourceGenHelpers.Tests;

                              static partial class TestPiFluentClass
                              {
                                  public static partial int GetPiDecimal(int decimalNumber)
                                  {
                                      switch (decimalNumber)
                                      {
                                          case 0: return 3;
                                          case 1: return 1;
                                          case 2: return 4;
                                          case 300: return 3;
                                          case 301: return 7;
                                          case 302: return 2;
                                          case 303: return 4;
                                          default: return TestSlowMath.CalculatePiDecimal(decimalNumber);
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }

    [TestCase(1, "Dog")]
    [TestCase(2, "Cat")]
    [TestCase(3, "Unknown")]
    public void MapperFluentLikeGenerator_ProducesExpectedRuntimeOutput(int source, string expected)
    {
        string result = TestMapperFluent.MapToMammal(source);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void MapperFluentLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestMapperFluent_MapToMammal.g.cs");
        string expectedCode = """
                              namespace MattSourceGenHelpers.Tests;

                              static partial class TestMapperFluent
                              {
                                  public static partial string MapToMammal(int animalCode)
                                  {
                                      switch (animalCode)
                                      {
                                          case 1: return "Dog";
                                          case 2: return "Cat";
                                          default: return "Unknown";
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class TestPiFluentClass
{
    public static partial int GetPiDecimal(int decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    static IMethodImplementationGenerator GetPiDecimal_Generator() =>
        Generate
            .Method().WithParameter<int>().WithReturnType<int>()
            .WithSwitchBody()
            .ForCases(0, 1, 2, Integer.Range(300, 303)).ReturnConstantValue(decimalNumber => TestSlowMath.CalculatePiDecimal(decimalNumber))
            .ForDefaultCase().WithBody(decimalNumber => () => TestSlowMath.CalculatePiDecimal(decimalNumber));
}

public static partial class TestMapperFluent
{
    public static partial string MapToMammal(int animalCode);

    [GeneratesMethod(nameof(MapToMammal))]
    static IMethodImplementationGenerator MapToMammal_Generator() =>
        Generate
            .Method().WithParameter<int>().WithReturnType<string>()
            .WithSwitchBody()
            .ForCases(1).ReturnConstantValue(_ => "Dog")
            .ForCases(2).ReturnConstantValue(_ => "Cat")
            .ForDefaultCase().WithBody(_ => () => "Unknown");
}
