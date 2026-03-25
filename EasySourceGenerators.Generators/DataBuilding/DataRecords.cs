using System;

namespace EasySourceGenerators.Generators.DataBuilding;

public record BodyGenerationData(
    Type? ReturnType = null,
    Type[]? ParametersTypes = null,
    Delegate? RuntimeDelegateBody = null,
    Delegate? ReturnConstantValueFactory = null
    );