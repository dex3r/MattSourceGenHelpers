using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Tests;

public class Tests
{
    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedRuntimeOutput()
    {
        TestColorsClass testColorsClass = new();

        string allColors = testColorsClass.GetAllColorsString();

        Assert.That(allColors, Is.EqualTo("Red, Green, Blue"));
    }

    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCodePath = GetGeneratedCodePath();
        string generatedCode = File.ReadAllText(generatedCodePath).TrimStart('\uFEFF').ReplaceLineEndings("\n").TrimEnd();
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

    private static string GetGeneratedCodePath() =>
            Path.Combine(
            Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..")),
            "obj",
            "Debug",
            "net10.0",
            "generated",
            "MattSourceGenHelpers.Generators",
            "MattSourceGenHelpers.Generators.GeneratesMethodGenerator",
            "TestColorsClass_GetAllColorsString.g.cs");
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

    [GeneratesMethod(nameof(GetAllColorsString))]
    private static string GetAllColorsString_Generator() =>
        string.Join(", ", Enum.GetNames<TestColorsEnum>());
}
