using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Examples;

public partial class ColorsClassFluent
{
    public partial string GetAllColorsString();

    [GeneratesMethod(nameof(GetAllColorsString))]
    static IMethodImplementationGenerator GetAllColorsString_Generator() =>
        Generate
            .MethodImplementation<string>()
            .WithBody(() => string.Join(", ", Enum.GetNames<ColorsEnum>()));
}

/*
 This will generate the following method:

    public string GetAllColorsString()
    {
        return "Red, Green, Blue";
    }
*/