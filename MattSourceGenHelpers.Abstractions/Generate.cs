namespace MattSourceGenHelpers.Abstractions;

public static class Generate
{
    public static IGeneratorsFactory CurrentGenerator { get; set; } = new RecordingGeneratorsFactory();

    public static IMethodBuilder Method() => new MethodBuilder(CurrentGenerator);
}

public interface IMethodBuilder
{
    IMethodBuilder<TArg1> WithParameter<TArg1>();
    IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>();
}

public interface IMethodBuilder<TArg1>
{
    IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>();
}

public class MethodBuilder : IMethodBuilder
{
    private readonly IGeneratorsFactory _generatorsFactory;

    public MethodBuilder(IGeneratorsFactory generatorsFactory)
    {
        _generatorsFactory = generatorsFactory;
    }

    public IMethodBuilder<TArg1> WithParameter<TArg1>() => new MethodBuilder<TArg1>(_generatorsFactory);

    public IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>() => _generatorsFactory.CreateImplementation<TReturnType>();
}

public class MethodBuilder<TArg1> : IMethodBuilder<TArg1>
{
    private readonly IGeneratorsFactory _generatorsFactory;

    public MethodBuilder(IGeneratorsFactory generatorsFactory)
    {
        _generatorsFactory = generatorsFactory;
    }

    public IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>() => _generatorsFactory.CreateImplementation<TArg1, TReturnType>();
}

public class EmptyGeneratorsFactory : IGeneratorsFactory
{
    public IMethodImplementationGenerator CreateImplementation() => new EmptyMethodImplementationGenerator();
    public IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>() => new EmptyMethodImplementationGenerator<TReturnType>();
    public IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>() => new EmptyMethodImplementationGenerator<TArg1, TReturnType>();
}

public class EmptyMethodImplementationGenerator : IMethodImplementationGenerator
{
    public IMethodImplementationGenerator WithBody(Action body) => this;
    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
}

public class EmptyMethodImplementationGenerator<TReturnType> : IMethodImplementationGenerator<TReturnType>
{
    public IMethodImplementationGenerator UseBody(Func<object> body) => this;
}

public class EmptyMethodImplementationGenerator<TArg1, TReturnType> : IMethodImplementationGenerator<TArg1, TReturnType>
{
    public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> WithSwitchBody() =>
        new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(new SwitchBodyRecord());
    public IMethodImplementationGenerator WithBody(Action body) => this;
    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
}

public interface IGeneratorsFactory
{
    IMethodImplementationGenerator CreateImplementation();
    IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>();
    IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>();
}
