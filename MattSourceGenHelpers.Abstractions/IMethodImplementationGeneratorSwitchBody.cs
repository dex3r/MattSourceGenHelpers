namespace MattSourceGenHelpers.Abstractions;

public interface IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>
{
    IMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType> ForCases(params object[] cases);
    IMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase();
}