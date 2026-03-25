using EasySourceGenerators.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace EasySourceGenerators.Examples;

public static partial class PiExampleFluent
{
    public static partial int GetPiDecimal(int decimalNumber);

    [MethodBodyGenerator(nameof(GetPiDecimal))]
    static IMethodBodyGenerator GetPiDecimal_Generator() =>
        Generate.MethodBody()
            .ForMethod().WithReturnType<int>().WithParameter<int>()
            .WithCompileTimeConstants(() => new
            {
                PrecomputedTargets = (new int[] { 0, 1, 2, 300, 301, 302, 303 }).ToDictionary(i => i, i => SlowMath.CalculatePiDecimal(i))
            })
            .UseProvidedBody((constants, decimalNumber) =>
            {
                if (constants.PrecomputedTargets.TryGetValue(decimalNumber, out int precomputedResult)) return precomputedResult;
                
                return SlowMath.CalculatePiDecimal(decimalNumber);
            });
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
