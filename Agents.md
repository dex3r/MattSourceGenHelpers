# About this project
This is a dotnet 8 solution. The goal is to make using Roslyn Source Generators much easier.
With this tool, you can write your generator as a normal method, and it will be converted to a Source Generator.
This allows you to write your generator in a more natural way, without having to deal with the complexities of Roslyn Source Generators.

# Projects
 - .Examples - illustrative example of what this tool should do and how it's useful
 - .Generators - the actual Source Generators code
 - .Abstractions - things like attributes etc.; this should allow to leave generating code in the binaries after generation is done
 - .Tests - tests with Generators as Roslyn Source Generators
 - .GeneratorTests - tests with Generators as assembly references, to cover tests that would be impossible in .Tests

# Simple example

```
public enum ColorsEnum
{ 
    Red, Green, Blue
}

public partial class ColorsClass
{
    public partial string GetAllColorsString();

    [GeneratesMethod(nameof(GetAllColorsString))]
    static string GetAllColorsString_Generator() =>
        string.Join(", ", Enum.GetNames<ColorsEnum>());
}

/*
This will produce this code:
public class ColorsClass
{
    public string GetAllColorsString()
    {
        return "Red, Green, Blue";
    }
}
*/
```

# Coding rules
 - for every new feature, try adding unit tests
 - create integration tests as well, if old ones don't cover this case
 - create small, testable classes

# Code Style
 - don't use `var` when possible; use full type
