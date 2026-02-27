using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Tests;

public class Tests
{
    private const char UnicodeBom = '\uFEFF';

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
        string generatedCodePath = GetGeneratedCodePath();
        string generatedCode = File.ReadAllText(generatedCodePath).TrimStart(UnicodeBom).ReplaceLineEndings("\n").TrimEnd();
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

    private static string GetGeneratedCodePath()
    {
        string projectDirectory = FindProjectDirectory();
        string[] generatedFiles = Directory.GetFiles(projectDirectory, "TestColorsClass_GetAllColorsString.g.cs", SearchOption.AllDirectories);

        return generatedFiles.Single();
    }

    private static string FindProjectDirectory()
    {
        string? currentDirectory = TestContext.CurrentContext.TestDirectory;

        while (currentDirectory is not null)
        {
            string projectFilePath = Path.Combine(currentDirectory, "MattSourceGenHelpers.Tests.csproj");
            if (File.Exists(projectFilePath))
            {
                return currentDirectory;
            }

            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate MattSourceGenHelpers.Tests project directory.");
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

    [GeneratesMethod(nameof(GetAllColorsString))]
    private static string GetAllColorsString_Generator() =>
        string.Join(", ", Enum.GetNames<TestColorsEnum>());
}
