namespace MattSourceGenHelpers.Abstractions;

public interface IMethodImplementationGenerator
{
    IMethodImplementationGenerator WithBody(Action body);
    IMethodImplementationGenerator WithBody(Func<object> body);
}