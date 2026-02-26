namespace MattSourceGenHelpers.Abstractions;

public interface IMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> CompileTimeBody(Func<TArg1, TReturnType> func);
    IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> RuntimeBody(Func<TArg1, Action<TReturnType>> func);
}