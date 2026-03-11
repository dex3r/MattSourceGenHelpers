namespace EasySourceGenerators.Abstractions.Method;

public interface IMethodBuilderStage2ReturningVoid
{
    IMethodBodyBuilderStage4ReturnVoidNoArg WithNoParameters();
    
    IMethodBodyBuilderStage4ReturnVoid<TParam1> WithParameter<TParam1>();
    IMethodBodyBuilderStage4ReturnVoid<object> WithParameter(Type param1);
    IMethodBodyBuilderStage4ReturnVoid<object> WithParameter(string param1);
    IMethodBodyBuilderStage4ReturnVoid<object> WithParameter(Func<Type> param1Factory);
    IMethodBodyBuilderStage4ReturnVoid<object> WithParameter(Func<string> param1Factory);
}

public interface IMethodBuilderStage2<TReturnType>
{
    IMethodBodyBuilderStage4NoArg<TReturnType> WithNoParameters();
    
    IMethodBodyBuilderStage4<TParam1, TReturnType> WithParameter<TParam1>();
    IMethodBodyBuilderStage4<object, TReturnType> WithParameter(Type param1);
    IMethodBodyBuilderStage4<object, TReturnType> WithParameter(string param1);
    IMethodBodyBuilderStage4<object, TReturnType> WithParameter(Func<Type> param1Factory);
    IMethodBodyBuilderStage4<object, TReturnType> WithParameter(Func<string> param1Factory);
}