namespace EasySourceGenerators.Abstractions;

public interface IMethodImplementationGenerator<[UsedImplicitly] TReturnType> : IMethodImplementationGenerator
{
    IMethodImplementationGenerator UseBody([UsedImplicitly] Func<object> body);
}

public interface IMethodImplementationGenerator<TArg1, TReturnType> : IMethodImplementationGenerator
{
    IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> WithSwitchBody();
}

public interface IMethodImplementationGenerator;
