# MattSourceGenHelpers

[![codecov](https://codecov.io/gh/dex3r/MattSourceGenHelpers/branch/main/graph/badge.svg)](https://codecov.io/gh/dex3r/MattSourceGenHelpers)

With MattSourceGenHelpers, you can write your generator code as a normal method, in the same assembly as the output.
This allows you to write your generator in a more natural way, without having to deal with the complexities of Roslyn Source Generators and a separate Project.

## What it does

You declare partial methods and provide generator methods marked with attributes such as:
- `[GeneratesMethod(...)]`
- `[SwitchCase(...)]`
- `[SwitchDefault]`

The generator then emits the final method body at compile time.

## Example: simple method generation

```csharp
public enum ColorsEnum { Red, Green, Blue }

public partial class ColorsClass
{
    public partial string GetAllColorsString();

    [GeneratesMethod(nameof(GetAllColorsString))]
    static string GetAllColorsString_Generator() =>
        string.Join(", ", Enum.GetNames<ColorsEnum>());
}
```

This generates a method:

```csharp
public string GetAllColorsString() => "Red, Green, Blue";
```

## Example: switch-case specialization with fallback

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

This generates a method:

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

## Example: fluent API

Instead of using attributes, you can use the fluent API to build more complex behaviour:

```csharp
public enum FourLegged { Dog, Cat, Lizard }
public enum Mammal { Dog, Cat }

public static partial class MapperFluent
{
    public static partial Mammal MapToMammal(FourLegged fourLegged);

    [GeneratesMethod(nameof(MapToMammal))]
    static IMethodImplementationGenerator MapToAnimal_Generator() =>
        Generate
            .Method().WithParameter<FourLegged>().WithReturnType<Mammal>()
            .WithSwitchBody()
            .ForCases(GetFourLeggedAnimalsThatHasMatchInMammalAnimal()).ReturnConstantValue(fourLegged => Enum.Parse<Mammal>(fourLegged.ToString(), true))
            .ForDefaultCase().UseBody(fourLegged => () => throw new ArgumentException($"Cannot map {fourLegged} to a Mammal"));

    static FourLegged[] GetFourLeggedAnimalsThatHasMatchInMammalAnimal() =>
        Enum
            .GetValues<FourLegged>()
            .Where(fourLeggedAnimal => Enum.TryParse(typeof(Mammal), fourLeggedAnimal.ToString(), true, out _))
            .ToArray();
}
```

This generates a method:

```csharp
public static partial Mammal MapToMammal(FourLegged fourLegged)
{
    switch (fourLegged)
    {
        case FourLegged.Dog: return Mammal.Dog;
        case FourLegged.Cat: return Mammal.Cat;
        default: throw new ArgumentException($"Cannot map {fourLegged} to a Mammal");
    }
}
```

## Great IDE support

This project uses Roslyn Source Generators under the hood. This gives you great out-of-the-box support from IDEs like Rider, VSCode, and VisualStudio. 

You can browse the generated code live in the `IL Viewer` window of your IDE:

<img width="1037" height="397" alt="image" src="https://github.com/user-attachments/assets/1affb888-04d4-4fc4-8e79-db3e485de30c" />

## More examples

See the full example project here: [`/MattSourceGenHelpers.Examples`](./MattSourceGenHelpers.Examples).

## Structure 

The project is split into:
- `MattSourceGenHelpers.Abstractions` - attributes and fluent interfaces used in consumer code
- `MattSourceGenHelpers.Generators` - the source generator implementation
- `MattSourceGenHelpers.Examples` - practical usage examples
- `MattSourceGenHelpers.Tests` - tests with Generators as Roslyn Source Generators
- `MattSourceGenHelpers.GeneratorTests` - tests with Generators as assembly references, to cover tests that would be impossible otherwise
