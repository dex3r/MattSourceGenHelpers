namespace MattSourceGenHelpers.Abstractions;

public interface IMethodImplementationGenerator<TReturnType> : IMethodImplementationGenerator
{
    IMethodImplementationGenerator WithBody(Func<object> body);
}

public interface IMethodImplementationGenerator<TArg1, TReturnType> : IMethodImplementationGenerator
{
    IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> WithSwitchBody();
}

public interface IMethodImplementationGeneratorVoid
{
    IMethodImplementationGenerator CompileTimeBody(Action func);
    IMethodImplementationGenerator RuntimeBody(Action func);
}

public interface IMethodImplementationGenerator;