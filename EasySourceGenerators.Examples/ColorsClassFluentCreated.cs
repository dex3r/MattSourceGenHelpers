using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Examples;

public partial class ColorsClassFluentCreated
{
    [MethodBodyGenerator("GetAllColorsString")]
    static IMethodBodyGenerator GetAllColorsString_Generator() =>
        Generate.Method()
            .WithName("GetAllColorsString")
            .WithReturnType<string>()
            .WithNoParameters()
            .BodyReturningConstant(() => string.Join(", ", Enum.GetNames<ColorsEnum>()));
}

/*
 This will generate the following method:

    public string GetAllColorsString()
    {
        return "Red, Green, Blue";
    }
*/
