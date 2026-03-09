namespace EasySourceGenerators.Abstractions.Method;

public interface IMethodBuilderStage1
{
    IMethodBuilderStage2 WithName(string name);
    IMethodBuilderStage2 WithName(Func<string> nameFactory);
}

public interface IMethodBuilderStage2
{
    IMethodBuilderStage3ReturningVoid WithVoidReturn();
    
    IMethodBuilderStage3<TReturnType> WithReturnType<TReturnType>();
    IMethodBuilderStage3WithSomeReturnType WithReturnType(Type returnType);
    IMethodBuilderStage3WithSomeReturnType WithReturnType(string returnType);
    IMethodBuilderStage3WithSomeReturnType WithReturnType(Func<Type> returnTypeFactory);
    IMethodBuilderStage3WithSomeReturnType WithReturnType(Func<string> returnTypeFactory);
}

public interface IMethodBuilderStage3ReturningVoid
{
    IMethodBuilderStage4ReturnVoidNoParam WithNoParameters();
    
    IMethodBuilderStage4ReturnVoid<TParam1> WithParameter<TParam1>();
    IMethodBuilderStage4ReturnVoid WithParameter(Type param1);
    IMethodBuilderStage4ReturnVoid WithParameter(string param1);
    IMethodBuilderStage4ReturnVoid WithParameter(Func<Type> param1Factory);
    IMethodBuilderStage4ReturnVoid WithParameter(Func<string> param1Factory);
}

public interface IMethodBuilderStage3<TReturnType>
{
    IMethodBuilderStage4NoParam WithNoParameters();
    
    IMethodBuilderStage4<TParam1> WithParameter<TParam1>();
    IMethodBuilderStage4 WithParameter(Type param1);
    IMethodBuilderStage4 WithParameter(string param1);
    IMethodBuilderStage4 WithParameter(Func<Type> param1Factory);
    IMethodBuilderStage4 WithParameter(Func<string> param1Factory);
}

public interface IMethodBuilderStage3WithSomeReturnType
{
    IMethodBuilderStage4WithSomeReturnTypeNoParam WithNoParameters();
    
    IMethodBuilderStage4WithSomeReturnType<TParam1> WithParameter<TParam1>();
    IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Type param1);
    IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(string param1);
    IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Func<Type> param1Factory);
    IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Func<string> param1Factory);
}

public interface IMethodBuilderStage4ReturnVoidNoParam
{
    
}