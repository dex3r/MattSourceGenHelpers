using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Examples;

public enum FourLegged
{
    Dog, Cat, Lizard
}

public enum Mammal
{
    Dog, Cat
}

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

/*
 This will generate the following method:

    public static partial Mammal MapToMammal(FourLegged fourLegged)
    {
        switch (fourLegged)
        {
            case FourLegged.Dog: return Mammal.Dog;
            case FourLegged.Cat: return Mammal.Cat;
            default: throw new ArgumentException($"Cannot map {fourLegged} to a Mammal");
        }
    }
*/