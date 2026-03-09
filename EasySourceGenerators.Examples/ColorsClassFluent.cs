using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Examples;

public partial class ColorsClassFluent
{
    public partial string GetAllColorsString();

    [MethodBodyGenerator(nameof(GetAllColorsString))]
    static IMethodImplementationGenerator GetAllColorsString_Generator() =>
        Generate
            .Method().WithReturnType<string>()
            .UseBody(() => string.Join(", ", Enum.GetNames<ColorsEnum>()));
}

/*
 This will generate the following method:

    public string GetAllColorsString()
    {
        return "Red, Green, Blue";
    }
*/
