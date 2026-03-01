using MattSourceGenHelpers.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace MattSourceGenHelpers.Examples;

public static partial class PiExampleFluent
{
    public static partial int GetPiDecimal(int decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    static IMethodImplementationGenerator GetPiDecimal_Generator_Specialized() =>
        Generate
            .Method().WithParameter<int>().WithReturnType<int>()
            .WithSwitchBody()
            .ForCases(0, 1, 2, Integer.Range(300, 303)).ReturnConstantValue(decimalNumber => SlowMath.CalculatePiDecimal(decimalNumber))
            .ForDefaultCase().UseBody(decimalNumber => () => SlowMath.CalculatePiDecimal(decimalNumber));
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
            case 300: return 3;
            case 301: return 7;
            case 302: return 2;
            case 303: return 4;
            default: return CalculatePiDecimal(decimalNumber);
        }
    }
*/
