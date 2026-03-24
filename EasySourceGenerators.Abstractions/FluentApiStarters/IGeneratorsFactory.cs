using EasySourceGenerators.Abstractions.Method;
using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

[PublicAPI]
public interface IGeneratorsFactory
{
    IMethodBodyBuilderStage1 StartFluentApiBuilderForBody();
    IMethodBuilderStage1 StartFluentApiBuilderForMethod(Func<string> methodNameFactory);
}