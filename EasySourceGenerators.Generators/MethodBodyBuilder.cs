using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Generators;

//TODO: A lot of interfaces, like IMethodBodyGeneratorStage0, has been refactored or changed entirely. But the Examples remained the same. Fix the generators.
public class MethodBodyBuilder(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilder
{
    public IMethodBodyBuilder<TArg1> WithParameter<TArg1>() => new MethodBodyBodyBuilder<TArg1>(generatorsFactory);

    public IMethodBodyGenerator<TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TReturnType>();
}

public class MethodBodyBodyBuilder<TArg1>(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilder<TArg1>
{
    public IMethodBodyGenerator<TArg1, TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TArg1, TReturnType>();
}
