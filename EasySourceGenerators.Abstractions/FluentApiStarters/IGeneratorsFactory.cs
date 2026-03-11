using EasySourceGenerators.Abstractions.Method;

namespace EasySourceGenerators.Abstractions;

public interface IGeneratorsFactory
{
    IMethodBodyBuilderStage1 StartFluentApiBuilderForBody();
    IMethodBuilderStage1 StartFluentApiBuilderForMethod(Func<string> methodNameFactory);
}