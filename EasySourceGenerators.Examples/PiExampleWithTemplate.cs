using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Examples;

public static partial class PiExampleWithTemplate
{
    public static partial int GetPiDecimal(int decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    [MethodTemplate]
    static int GetPiDecimal_Template(int decimalNumber)
    {
        switch (decimalNumber)
        {
            case 0: return TemplateEngine.RunAtCompileTime(() => SlowMath.CalculatePiDecimal(0));
            case 1: return TemplateEngine.RunAtCompileTime(() => SlowMath.CalculatePiDecimal(1));
            case 2: return TemplateEngine.RunAtCompileTime(() => SlowMath.CalculatePiDecimal(2));
            default: return CalculatePiDecimal(decimalNumber);
        }
    }
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
            default: return CalculatePiDecimal(decimalNumber);
        }
    }
*/