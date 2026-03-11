using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Examples;

public partial class ColorsClassFluentCreated
{
    [MethodGenerator]
    static IMethodBodyGenerator GetAllColorsString_Generator() =>
        Generate
            .Method("GetAllColorsStringGenerated").WithReturnType<string>().WithNoParameters()
            .BodyReturningConstant(() => string.Join(", ", Enum.GetNames<ColorsEnum>()));
}

/*
 This will generate the following method:

    public string GetAllColorsStringGenerated()
    {
        return "Red, Green, Blue";
    }
*/
