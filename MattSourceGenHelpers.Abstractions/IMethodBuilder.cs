namespace MattSourceGenHelpers.Abstractions;

public interface IMethodBuilder
{
    IMethodBuilder<TArg1> WithParameter<TArg1>();
    IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>();
}

public interface IMethodBuilder<TArg1>
{
    IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>();
}