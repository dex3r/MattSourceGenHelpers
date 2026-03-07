# EasySourceGenerators

[![NuGet](https://img.shields.io/nuget/v/EasySourceGenerators.Generators?logo=nuget)](https://www.nuget.org/packages/EasySourceGenerators.Generators/)
[![MIT License](https://img.shields.io/github/license/dex3r/EasySourceGenerators)](https://github.com/dex3r/EasySourceGenerators/blob/main/LICENSE)
[![GitHub Repo](https://img.shields.io/badge/GitHub-dex3r%2FEasySourceGenerators-181717?logo=github)](https://github.com/dex3r/EasySourceGenerators)
[![CI](https://github.com/dex3r/EasySourceGenerators/actions/workflows/ci.yml/badge.svg)](https://github.com/dex3r/EasySourceGenerators/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/dex3r/EasySourceGenerators/branch/main/graph/badge.svg)](https://codecov.io/gh/dex3r/EasySourceGenerators)

**Easy Source Generators** - Code generation made easy.

With this package, you can easily create **code that generates source** - without creating a separate Analyzers assembly and without learning Roslyn Source Generators. It will be generated any time you run a build.

Just look at the examples below, or scroll down to a more in-depth explanation.

# Examples
### Simple method generation

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

### Switch-case specialization with fallback

```csharp
public static partial class PiExample
{
    public static partial int GetPiDecimal(int decimalNumber);

    [GeneratesMethod(nameof(GetPiDecimal))]
    [SwitchCase(0)]
    [SwitchCase(1)]
    [SwitchCase(2)]
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

### Fluent API

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
public static Mammal MapToMammal(FourLegged fourLegged)
{
    switch (fourLegged)
    {
        case FourLegged.Dog: return Mammal.Dog;
        case FourLegged.Cat: return Mammal.Cat;
        default: throw new ArgumentException($"Cannot map {fourLegged} to a Mammal");
    }
}
```

#### More examples
See the full example project here: [`/EasySourceGenerators.Examples`](./EasySourceGenerators.Examples).

## What it does

You declare partial methods, either static or non-static, and provide generator methods marked with attributes such as:
- `[GeneratesMethod(nameof(YourMethod)]`
- `[SwitchCase("someArgumentValue)]`
- `[SwitchDefault]`

The generator uses Roslyn Source Generators under the hood to generate the source at build time.

For more complex behavior there is a fluent API as well (see `MapperFluent` example above).

The Generators package and its binaries will not be included in your shipped code. The generators package will be added as a compile-time only dependency:

```xml
  <PackageReference Include="EasySourceGenerators.Generators" Version="x.y.z"
                    PrivateAssets="all"
                    IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
```

## Great IDE support

This project uses Roslyn Source Generators under the hood. This gives you great out-of-the-box support from IDEs like Rider, VSCode, and VisualStudio. 

You can browse the generated code live in the `IL Viewer` window of your IDE:

<img width="1037" height="397" alt="image" src="https://github.com/user-attachments/assets/1affb888-04d4-4fc4-8e79-db3e485de30c" />

## Verbose and easy to understand errors

Remove the guesswork from Source Generators.

Roslyn Source Generator projects often produce errors that are hard to understand. One of the main goals of this package is to make sure that any error - whether it’s a user mistake, a setup problem, or an edge case - shows up clearly in your IDE by highlighting the line that caused it.

<img width="626" height="98" alt="image" src="https://github.com/user-attachments/assets/cfe7544f-05b8-4e33-bacd-a35c23a5bae7" />

## Structure 

The project is split into:
- `EasySourceGenerators.Abstractions` - attributes and fluent interfaces used in consumer code
- `EasySourceGenerators.Generators` - the source generator implementation
- `EasySourceGenerators.Examples` - practical usage examples
- `EasySourceGenerators.Tests` - tests with Generators as Roslyn Source Generators
- `EasySourceGenerators.GeneratorTests` - tests with Generators as assembly references, to cover tests that would be impossible otherwise
