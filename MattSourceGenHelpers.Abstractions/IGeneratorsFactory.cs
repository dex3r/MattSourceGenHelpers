namespace MattSourceGenHelpers.Abstractions;

public interface IGeneratorsFactory
{
    IMethodBuilder ForMethod();
    
    IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>();
    IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>();
}