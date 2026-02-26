namespace MattSourceGenHelpers.Abstractions;

public static class Generator
{
    public static IGeneratorsFactory CurrentGenerator { get; set; } = new RecordingGeneratorsFactory();

    public static IMethodImplementationGenerator MethodImplementation() => CurrentGenerator.CreateImplementation();
    public static IMethodImplementationGenerator<TReturnType> MethodImplementation<TReturnType>() => (IMethodImplementationGenerator<TReturnType>)CurrentGenerator.CreateImplementation();
    public static IMethodImplementationGenerator<TArg1, TReturnType> MethodImplementation<TArg1, TReturnType>() => CurrentGenerator.CreateImplementation<TArg1, TReturnType>();
}

public class EmptyGeneratorsFactory : IGeneratorsFactory
{
    public IMethodImplementationGenerator CreateImplementation() => new EmptyMethodImplementationGenerator();
    public IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>() => new RecordingMethodImplementationGenerator<TArg1, TReturnType>(new SwitchBodyRecord());
}

public class EmptyMethodImplementationGenerator : IMethodImplementationGenerator
{
    public IMethodImplementationGenerator WithBody(Action body) => this;
    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
}

public interface IGeneratorsFactory
{
    IMethodImplementationGenerator CreateImplementation();
    IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>();
}