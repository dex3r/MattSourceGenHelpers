using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators.DataBuilding;

// ReSharper disable InconsistentNaming - names like IFoo are neede here

namespace EasySourceGenerators.Generators;

public static class Consts
{
    public const string SolutionNamespace = $"{nameof(EasySourceGenerators)}";
    public const string AbstractionsNamespace = $"{nameof(EasySourceGenerators)}.{nameof(Abstractions)}";
    public const string AbstractionsAssemblyName = AbstractionsNamespace;
    public const string GeneratorsNamespace = "EasySourceGenerators.Generators";
    public const string GeneratorsAssemblyName = GeneratorsNamespace;
    public const string SwitchCaseAttributeFullName = $"{AbstractionsNamespace}.{nameof(SwitchCase)}";
    public const string SwitchDefaultAttributeFullName = $"{AbstractionsNamespace}.{nameof(SwitchDefault)}";
    public const string GeneratesMethodAttributeFullName = $"{AbstractionsNamespace}.{nameof(MethodBodyGenerator)}";
    public const string IMethodImplementationGeneratorFullName = $"{AbstractionsNamespace}.{nameof(IMethodBodyGenerator)}";
    public const string GenerateTypeFullName = $"{AbstractionsNamespace}.{nameof(Generate)}";
    public const string DataGeneratorsFactoryTypeFullName = $"{GeneratorsNamespace}.DataBuilding.{nameof(DataGeneratorsFactory)}";
    public const string DataMethodBodyGeneratorTypeName = nameof(DataMethodBodyGenerator);
    public const string BodyGenerationDataPropertyName = "Data";
    public const string CurrentGeneratorPropertyName = nameof(Generate.CurrentGenerator);
}
