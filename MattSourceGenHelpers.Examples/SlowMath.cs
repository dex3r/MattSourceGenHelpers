using System.Globalization;
using ExtendedNumerics;

namespace MattSourceGenHelpers.Examples;

public static class SlowMath
{
    public static int CalculatePiDecimal(int decimalNumber)
    {
        if (decimalNumber < 0) throw new ArgumentOutOfRangeException(nameof(decimalNumber), "Decimal number must be non-negative.");
        if (decimalNumber == 0) return 3;
        
        BigDecimal pi = BigDecimal.ApproximatePi(decimalNumber + 1);

        if(pi.DecimalPlaces < decimalNumber) throw new ArgumentException($"Failed to calculate pi to {decimalNumber} decimal places.");

        string piString = pi.ToString(CultureInfo.InvariantCulture);
        return int.Parse(piString.Substring(decimalNumber + 1, 1));
    }
}