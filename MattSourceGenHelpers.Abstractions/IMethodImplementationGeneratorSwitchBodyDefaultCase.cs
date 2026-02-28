namespace MattSourceGenHelpers.Abstractions;

public interface IMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> : IMethodImplementationGenerator
{
    IMethodImplementationGenerator<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> func);
    IMethodImplementationGenerator<TArg1, TReturnType> UseBody(Func<TArg1, Func<TReturnType>> func);
}