# MattSourceGenHelpers

MattSourceGenHelpers is a set of helpers for building Roslyn Source Generator scenarios with less boilerplate.

The project is split into:
- `MattSourceGenHelpers.Abstractions` - attributes and fluent interfaces used in consumer code
- `MattSourceGenHelpers.Generators` - the source generator implementation
- `MattSourceGenHelpers.Examples` - practical usage examples

## What it does

You declare partial methods and provide generator methods marked with attributes such as:
- `[GeneratesMethod(...)]`
- `[SwitchCase(...)]`
- `[SwitchDefault]`

The generator then emits the final method body at compile time.

## Example: simple method generation

Inspired by `/MattSourceGenHelpers.Examples/ColorsClass.cs`:

```csharp
public enum ColorsEnum
{
    Red, Green, Blue
}

public partial class ColorsClass
{
    public partial string GetAllColorsString();

    [GeneratesMethod(nameof(GetAllColorsString))]
    static string GetAllColorsString_Generator() =>
        string.Join(", ", Enum.GetNames<ColorsEnum>());
}
```

Generated method:

```csharp
public string GetAllColorsString()
{
    return "Red, Green, Blue";
}
```

## Example: switch-case specialization with fallback

Inspired by `/MattSourceGenHelpers.Examples/PiExample.cs`:

```csharp
public static partial class PiExample
{
    public static partial int GetPiDecimal(int decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    [SwitchCase(arg1: 0)]
    [SwitchCase(arg1: 1)]
    [SwitchCase(arg1: 2)]
    static int GetPiDecimal_Generator_Specialized(int decimalNumber) =>
        SlowMath.CalculatePiDecimal(decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    [SwitchDefault]
    static Func<int, int> GetPiDecimal_Generator_Fallback() =>
        decimalNumber => SlowMath.CalculatePiDecimal(decimalNumber);
}
```

Generated method shape:

```csharp
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
```

## More examples

See the full example project here: [`/MattSourceGenHelpers.Examples`](./MattSourceGenHelpers.Examples).
