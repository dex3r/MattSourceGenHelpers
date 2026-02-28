// using MattSourceGenHelpers.Abstractions;
//
// namespace MattSourceGenHelpers.Examples;
//
// public enum FourLeggedAnimal
// {
//     Dog, Cat, Lizard
// }
//
// public enum MammalAnimal
// {
//     Dog, Cat
// }
//
// public static class MapperFluent
// {
//     public static partial MammalAnimal MapToMammal(FourLeggedAnimal fourLeggedAnimal);
//
//     [GeneratesMethod(nameof(MapToMammal))]
//     static IMethodImplementationGenerator MapToAnimal_Generator() =>
//         Generate
//             .Method().WithParameter<FourLeggedAnimal>().WithReturnType<MammalAnimal>()
//             .WithSwitchBody()
//             .ForCases(GetFourLeggedAnimalsThatHasMatchInMammalAnimal()).CompileTimeBody(fourLeggedAnimal => Enum.Parse<MammalAnimal>(fourLeggedAnimal.ToString(), true))
//             .ForDefaultCase().RuntimeBody(fourLeggedAnimal => () => throw new ArgumentException($"Cannot map {fourLeggedAnimal} to a MammalAnimal"));
//
//     static FourLeggedAnimal[] GetFourLeggedAnimalsThatHasMatchInMammalAnimal() =>
//         Enum
//             .GetValues<FourLeggedAnimal>()
//             .Where(fourLeggedAnimal => Enum.TryParse(typeof(MammalAnimal), fourLeggedAnimal.ToString(), true, out _))
//             .ToArray();
// }
//
// /*
//  This will generate the following method:
//
//     public static Animal MapToAnimal(FourLeggedAnimal fourLeggedAnimal);
//     {
//         switch (fourLeggedAnimal)
//         {
//             case FourLeggedAnimal.Dog: return MammalAnimal.Dog;
//             case FourLeggedAnimal.Cat: return MammalAnimal.Cat;
//             default: throw new ArgumentException($"Cannot map {fourLeggedAnimal} to a MammalAnimal");
//         }
//     }
// */