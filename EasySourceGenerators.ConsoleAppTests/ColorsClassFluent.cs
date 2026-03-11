using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.ConsoleAppTests;

public partial class ColorsClassFluent
{
    public partial string GetAllColorsString();

    [MethodBodyGenerator(nameof(GetAllColorsString))]
    static IMethodBodyGenerator GetAllColorsString_Generator() =>
        Generate
            .MethodBody().ForMethod().WithReturnType<string>().WithNoParameters()
            .BodyReturningConstant(() => string.Join(", ", Enum.GetNames<ColorsEnum>()));
}

/*
 This will generate the following method:

    public string GetAllColorsString()
    {
        return "Red, Green, Blue";
    }
*/
