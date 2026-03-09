using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.ConsoleAppTests;

public enum ColorsEnum
{ 
    Red, Green, Blue
}

public partial class ColorsClass
{
    public partial string GetAllColorsString();

    [MethodBodyGenerator(nameof(GetAllColorsString))]
    static string GetAllColorsString_Generator() =>
        string.Join(", ", Enum.GetNames<ColorsEnum>());
}