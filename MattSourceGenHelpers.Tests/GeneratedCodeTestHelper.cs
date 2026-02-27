namespace MattSourceGenHelpers.Tests;

internal static class GeneratedCodeTestHelper
{
    private const char UnicodeBom = '\uFEFF';

    internal static string ReadGeneratedCode(string generatedFileName)
    {
        string generatedCodePath = GetGeneratedCodePath(generatedFileName);
        return File.ReadAllText(generatedCodePath).TrimStart(UnicodeBom).ReplaceLineEndings("\n").TrimEnd();
    }

    private static string GetGeneratedCodePath(string generatedFileName)
    {
        string projectDirectory = FindProjectDirectory();
        string[] generatedFiles = Directory.GetFiles(projectDirectory, generatedFileName, SearchOption.AllDirectories);

        if (generatedFiles.Length == 0)
        {
            throw new AssertionException($"Could not find expected generated file '{generatedFileName}' under {projectDirectory}{Path.DirectorySeparatorChar}**");
        }

        return generatedFiles[0];
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
