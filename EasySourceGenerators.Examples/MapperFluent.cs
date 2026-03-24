//TODO: Replace SwitchCase with new version

// using EasySourceGenerators.Abstractions;
//
// namespace EasySourceGenerators.Examples;
//
// public enum FourLegged
// {
//     Dog, Cat, Lizard
// }
//
// public enum Mammal
// {
//     Dog, Cat
// }
//
// public static partial class MapperFluent
// {
//     public static partial Mammal MapToMammal(FourLegged fourLegged);
//
//     [MethodBodyGenerator(nameof(MapToMammal))]
//     static IMethodBodyGenerator MapToAnimal_Generator() =>
//         Generate.MethodBody()
//             .ForMethod().WithReturnType<Mammal>().WithParameter<FourLegged>()
//             .BodyWithSwitchStatement()
//             .ForCases(GetFourLeggedAnimalsThatHasMatchInMammalAnimal()).ReturnConstantValue(fourLegged => Enum.Parse<Mammal>(fourLegged.ToString(), true))
//             .ForDefaultCase().UseProvidedBody(fourLegged => throw new ArgumentException($"Cannot map {fourLegged} to a Mammal"));
//
//     static FourLegged[] GetFourLeggedAnimalsThatHasMatchInMammalAnimal() =>
//         Enum
//             .GetValues<FourLegged>()
//             .Where(fourLeggedAnimal => Enum.TryParse(typeof(Mammal), fourLeggedAnimal.ToString(), true, out _))
//             .ToArray();
// }
//
// /*
//  This will generate the following method:
//
//     public static partial Mammal MapToMammal(FourLegged fourLegged)
//     {
//         switch (fourLegged)
//         {
//             case FourLegged.Dog: return Mammal.Dog;
//             case FourLegged.Cat: return Mammal.Cat;
//             default: throw new ArgumentException($"Cannot map {fourLegged} to a Mammal");
//         }
//     }
// */