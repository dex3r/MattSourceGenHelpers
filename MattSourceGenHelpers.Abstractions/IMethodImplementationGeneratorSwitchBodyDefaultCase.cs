namespace MattSourceGenHelpers.Abstractions;

public interface IMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> : IMethodImplementationGenerator
{
    IMethodImplementationGenerator<TArg1, TReturnType> CompileTimeBody(Func<TArg1, TReturnType> func);
    IMethodImplementationGenerator<TArg1, TReturnType> RuntimeBody(Func<TArg1, Func<TReturnType>> func);
}