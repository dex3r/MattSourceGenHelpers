using MattSourceGenHelpers.Abstractions;

// ReSharper disable InconsistentNaming - names like IFoo are neede here

namespace MattSourceGenHelpers.Generators;

public static class Consts
{
    public const string AbstractionsNamespace = $"{nameof(MattSourceGenHelpers)}.{nameof(Abstractions)}";
    public const string AbstractionsAssemblyName = AbstractionsNamespace;
    public const string SwitchCaseAttributeFullName = $"{AbstractionsNamespace}.{nameof(SwitchCase)}";
    public const string SwitchDefaultAttributeFullName = $"{AbstractionsNamespace}.{nameof(SwitchDefault)}";
    public const string GeneratesMethodAttributeFullName = $"{AbstractionsNamespace}.{nameof(GeneratesMethod)}";
    public const string IMethodImplementationGeneratorFullName = $"{AbstractionsNamespace}.{nameof(IMethodImplementationGenerator)}";
    public const string GenerateTypeFullName = $"{AbstractionsNamespace}.{nameof(Generate)}";
    public const string RecordingGeneratorsFactoryTypeFullName = $"{AbstractionsNamespace}.{nameof(RecordingGeneratorsFactory)}";
    public const string CurrentGeneratorPropertyName = nameof(Generate.CurrentGenerator);
    public const string LastRecordPropertyName = nameof(RecordingGeneratorsFactory.LastRecord);
}
