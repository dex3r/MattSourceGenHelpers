# About this project
This is a dotnet 10 solution. The goal is to make using Roslyn Source Generators much easier.

# Project structure
 - .Examples - illustrative example of what this tool should do and how it's useful
 - .Generators - the actual Source Generators code
 - .Abstractions - things like attributes etc.; this should allow to leave generating code in the binaries after generation is done

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
