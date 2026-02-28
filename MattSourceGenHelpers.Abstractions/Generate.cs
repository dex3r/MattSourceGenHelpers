namespace MattSourceGenHelpers.Abstractions;

public static class Generate
{
    public static IGeneratorsFactory CurrentGenerator { get; set; } = new RecordingGeneratorsFactory();

    public static IMethodImplementationGenerator MethodImplementation() => CurrentGenerator.CreateImplementation();
    public static IMethodImplementationGenerator<TReturnType> MethodImplementation<TReturnType>() => CurrentGenerator.CreateImplementation<TReturnType>();
    public static IMethodImplementationGenerator<TArg1, TReturnType> MethodImplementation<TArg1, TReturnType>() => CurrentGenerator.CreateImplementation<TArg1, TReturnType>();
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
    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
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