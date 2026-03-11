using EasySourceGenerators.Abstractions.Method;
using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

public static class Generate
{
    internal static IGeneratorsFactory CurrentGenerator { get; [UsedImplicitly] set; } = new MockGeneratorsFactory();

    public static IMethodBodyBuilderStage1 MethodBody() => CurrentGenerator.StartFluentApiBuilderForBody();

    public static IMethodBuilderStage1 Method() => CurrentGenerator.StartFluentApiBuilderForMethod();
}