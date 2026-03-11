namespace EasySourceGenerators.Abstractions;

public interface IMethodBodyGenerator;

public interface IMethodBodyGeneratorWithNoParameter : IMethodBodyGenerator;

public interface IMethodBodyGenerator<TReturnType> : IMethodBodyGenerator
{
    IMethodBodyGeneratorWithNoParameter BodyReturningConstantValue(Func<object> body);
}

public interface IMethodBodyGenerator<TArg1, TReturnType> : IMethodBodyGenerator
{
    IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> GenerateSwitchBody();
}

public interface IMethodBodyGeneratorStage0
{
    IMethodBodyGeneratorWithNoParameter CreateImplementation();
    IMethodBodyGenerator<TReturnType> CreateImplementation<TReturnType>();
    IMethodBodyGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>();
    IMethodBodyBuilder ForMethod();
}

public interface IMethodBodyBuilder
{
    IMethodBodyBuilder<TArg1> WithParameter<TArg1>();
    IMethodBodyGenerator<TReturnType> WithReturnType<TReturnType>();
}

public interface IMethodBodyBuilder<TArg1>
{
    IMethodBodyGenerator<TArg1, TReturnType> WithReturnType<TReturnType>();
}