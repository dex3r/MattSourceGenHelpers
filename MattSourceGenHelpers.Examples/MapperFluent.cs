using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Examples;

public enum FourLeggedAnimal
{
    Dog, Cat, Lizard
}

public enum MammalAnimal
{
    Dog, Cat
}

public static partial class MapperFluent
{
    public static partial MammalAnimal MapToMammal(FourLeggedAnimal fourLeggedAnimal);

    [GeneratesMethod(nameof(MapToMammal))]
    static IMethodImplementationGenerator MapToAnimal_Generator() =>
        Generate
            .Method().WithParameter<FourLeggedAnimal>().WithReturnType<MammalAnimal>()
            .WithSwitchBody()
            .ForCases(GetFourLeggedAnimalsThatHasMatchInMammalAnimal()).ReturnConstantValue(fourLeggedAnimal => Enum.Parse<MammalAnimal>(fourLeggedAnimal.ToString(), true))
            .ForDefaultCase().UseBody(fourLeggedAnimal => () => throw new ArgumentException($"Cannot map {fourLeggedAnimal} to a MammalAnimal"));

    static FourLeggedAnimal[] GetFourLeggedAnimalsThatHasMatchInMammalAnimal() =>
        Enum
            .GetValues<FourLeggedAnimal>()
            .Where(fourLeggedAnimal => Enum.TryParse(typeof(MammalAnimal), fourLeggedAnimal.ToString(), true, out _))
            .ToArray();
}

/*
 This will generate the following method:

    public static partial MattSourceGenHelpers.Examples.MammalAnimal MapToMammal(MattSourceGenHelpers.Examples.FourLeggedAnimal fourLeggedAnimal)
    {
        switch (fourLeggedAnimal)
        {
            case MattSourceGenHelpers.Examples.FourLeggedAnimal.Dog: return MattSourceGenHelpers.Examples.MammalAnimal.Dog;
            case MattSourceGenHelpers.Examples.FourLeggedAnimal.Cat: return MattSourceGenHelpers.Examples.MammalAnimal.Cat;
            default: throw new ArgumentException($"Cannot map {fourLeggedAnimal} to a MammalAnimal");
        }
    }
*/