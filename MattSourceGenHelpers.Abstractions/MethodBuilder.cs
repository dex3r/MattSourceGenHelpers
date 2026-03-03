namespace MattSourceGenHelpers.Abstractions;

public class MethodBuilder(IGeneratorsFactory generatorsFactory) : IMethodBuilder
{
    public IMethodBuilder<TArg1> WithParameter<TArg1>() => new MethodBuilder<TArg1>(generatorsFactory);

    public IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TReturnType>();
}

public class MethodBuilder<TArg1>(IGeneratorsFactory generatorsFactory) : IMethodBuilder<TArg1>
{
    public IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TArg1, TReturnType>();
}