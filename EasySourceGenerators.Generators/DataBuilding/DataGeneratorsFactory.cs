using System;
using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Abstractions.Method;

namespace EasySourceGenerators.Generators.DataBuilding;

public class DataGeneratorsFactory : IGeneratorsFactory
{
    public IMethodBodyBuilderStage1 StartFluentApiBuilderForBody() => new DataMethodBodyBuilderStage1(new BodyGenerationData());
    
    public IMethodBuilderStage1 StartFluentApiBuilderForMethod(Func<string> methodNameFactory)
    {
        throw new NotImplementedException();
    }
}