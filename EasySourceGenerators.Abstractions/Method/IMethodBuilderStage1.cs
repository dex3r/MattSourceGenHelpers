namespace EasySourceGenerators.Abstractions.Method;

public interface IMethodBuilderStage1
{
    IMethodBuilderStage2ReturningVoid WithVoidReturn();
    
    IMethodBuilderStage2<TReturnType> WithReturnType<TReturnType>();
    IMethodBuilderStage2<object> WithReturnType(Type returnType);
    IMethodBuilderStage2<object> WithReturnType(string returnType);
    IMethodBuilderStage2<object> WithReturnType(Func<Type> returnTypeFactory);
    IMethodBuilderStage2<object> WithReturnType(Func<string> returnTypeFactory);
}