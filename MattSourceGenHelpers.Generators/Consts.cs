using MattSourceGenHelpers.Abstractions;

// ReSharper disable InconsistentNaming - names like IFoo are neede here

namespace MattSourceGenHelpers.Generators;

public static class Consts
{
    public const string AbstractionsNamespace = $"{nameof(MattSourceGenHelpers)}.{nameof(Abstractions)}";
    public const string SwitchCaseAttributeFullName = $"{AbstractionsNamespace}.{nameof(SwitchCase)}";
    public const string SwitchDefaultAttributeFullName = $"{AbstractionsNamespace}.{nameof(SwitchDefault)}";
    public const string GeneratesMethodAttributeFullName = $"{AbstractionsNamespace}.{nameof(GeneratesMethod)}";
    public const string IMethodImplementationGeneratorFullName = $"{AbstractionsNamespace}.{nameof(IMethodImplementationGenerator)}";
}