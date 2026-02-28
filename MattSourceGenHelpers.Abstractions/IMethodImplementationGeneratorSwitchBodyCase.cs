namespace MattSourceGenHelpers.Abstractions;

public interface IMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    /// <summary>
    /// Specify case(s) will return a constant value.
    /// </summary>
    /// <param name="constantValueFactory">During code generation, this delegate will be ran to calculate the constant value.
    /// The delegate will not be used in the generated code. Only the value it produces after it's executed during code generation.</param>
    IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> constantValueFactory);
    
    /// <summary>
    /// Specific case(s) will use the body provided.
    /// </summary>
    /// <param name="body">During code generation this body will be emitted.</param>
    IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> UseBody(Func<TArg1, Action<TReturnType>> body);
}