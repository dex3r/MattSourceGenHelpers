using EasySourceGenerators.Abstractions.Method;
using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

public static class Generate
{
    internal static IGeneratorsFactory CurrentGenerator
    {
        get => field ?? throw new InvalidOperationException("Trying to run Easy Source Generator in runtime or in environment other than" +
                                                            " Easy Source Generators internals. Or something went very wrong with" +
                                                            " Easy Source Generators internals.");
        [UsedImplicitly(ImplicitUseKindFlags.Assign, Reason = "Used in Easy Source Generators internals")] set;
    }

    public static IMethodBodyBuilderStage1 MethodBody() => CurrentGenerator.StartFluentApiBuilderForBody();

    public static IMethodBuilderStage1 Method(string methodName) => CurrentGenerator.StartFluentApiBuilderForMethod(() => methodName);
    public static IMethodBuilderStage1 Method(Func<string> methodNameFactory) => CurrentGenerator.StartFluentApiBuilderForMethod(methodNameFactory);
}