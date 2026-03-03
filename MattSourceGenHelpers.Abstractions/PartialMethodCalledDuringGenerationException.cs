using JetBrains.Annotations;

namespace MattSourceGenHelpers.Abstractions;

/// <summary>
/// Thrown when a partial method is called during source generation.
/// Partial methods cannot be invoked inside generator methods because they are the
/// methods being generated — they do not have an implementation yet at generation time.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class PartialMethodCalledDuringGenerationException(string methodName, string typeName) : InvalidOperationException(
    $"Partial method '{typeName}.{methodName}' was called during source generation. " +
    $"Partial methods cannot be invoked inside generator methods because their implementations " +
    $"are what is being generated. Remove the call to '{methodName}' from your generator method.");
