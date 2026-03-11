using EasySourceGenerators.Abstractions;

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
    public const string RecordingGeneratorsFactoryTypeFullName = $"{GeneratorsNamespace}.{nameof(RecordingGeneratorsFactory)}";
    public const string CurrentGeneratorPropertyName = nameof(Generate.CurrentGenerator);
    public const string LastRecordPropertyName = nameof(RecordingGeneratorsFactory.LastRecord);
    public const string LastMethodRecordPropertyName = nameof(RecordingGeneratorsFactory.LastMethodRecord);
}
