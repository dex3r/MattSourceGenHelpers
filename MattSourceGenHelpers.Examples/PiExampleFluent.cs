using MattSourceGenHelpers.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace MattSourceGenHelpers.Examples;

public static partial class PiExampleFluent
{
    public static partial int GetPiDecimal(int decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    static IMethodImplementationGenerator GetPiDecimal_Generator_Specialized() =>
        Generator
            .MethodImplementation<int, int>()
            .WithSwitchBody()
            .ForCases(0, 1, 2, Integer.Range(300, 303)).CompileTimeBody(decimalNumber => SlowMath.CalculatePiDecimal(decimalNumber))
            .ForDefaultCase().RuntimeBody(decimalNumber => () => SlowMath.CalculatePiDecimal(decimalNumber));
}

/*
 This will generate the following method:

    public static int GetPiDecimal(int decimalNumber)
    {
        switch (decimalNumber)
        {
            case 0: return 3;
            case 1: return 1;
            case 2: return 4;
            case 300: return 2;
            case 302: return 9;
            case 303: return 8;
            default: return CalculatePiDecimal(decimalNumber);
        }
    }
*/