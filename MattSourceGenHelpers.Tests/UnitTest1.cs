using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Tests;

public class Tests
{
    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedRuntimeOutput()
    {
        TestColorsClass testColorsClass = new TestColorsClass();

        string allColors = testColorsClass.GetAllColorsString();

        Assert.That(allColors, Is.EqualTo("Red, Green, Blue"));
    }

    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestColorsClass_GetAllColorsString.g.cs");
        string expectedCode = """
                              namespace MattSourceGenHelpers.Tests;

                              partial class TestColorsClass
                              {
                                  public partial string GetAllColorsString()
                                  {
                                      return "Red, Green, Blue";
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public enum TestColorsEnum
{
    Red,
    Green,
    Blue
}

public partial class TestColorsClass
{
    public partial string GetAllColorsString();

    [GeneratesMethod(sameClassMethodName: nameof(GetAllColorsString))]
    private static string GetAllColorsString_Generator() =>
        string.Join(", ", Enum.GetNames<TestColorsEnum>());
}
