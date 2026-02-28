using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Examples;

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
 This will generate the following method:

    public string GetAllColorsString()
    {
        return "Red, Green, Blue";
    }
*/