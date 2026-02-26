using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Examples;

public static class PiExample
{
    /// <summary>
    /// Returns Nth Pi decimal number. e.g. for 1 return 1, for 2 it returns 4, etc.
    /// </summary>
    public partial static int GetPiDecimal(int decimalNumber);
    
    [GeneratesMethod(nameof(GetPiDecimal))]
    static Func<int> GetPiDecimal_Generator(int decimalNumber);
}