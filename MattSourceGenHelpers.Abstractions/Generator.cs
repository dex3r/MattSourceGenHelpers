namespace MattSourceGenHelpers.Abstractions;

public static class Generator
{
    public static IGeneratorsFactory CurrentGenerator { get; set; } = new EmptyGeneratorsFactory();

    public static IMethodImplementationGenerator MethodImplementation() => CurrentGenerator.CreateImplementation();
}

public class EmptyGeneratorsFactory : IGeneratorsFactory
{
    public IMethodImplementationGenerator CreateImplementation() => new EmptyMethodImplementationGenerator();
}

public class EmptyMethodImplementationGenerator : IMethodImplementationGenerator
{
    public IMethodImplementationGenerator WithBody(Action body) => this;
    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
}

public interface IGeneratorsFactory
{
    IMethodImplementationGenerator CreateImplementation();
}