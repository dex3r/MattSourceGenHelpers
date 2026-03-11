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
    IMethodBuilderStage4NoParam<TReturnType> WithNoParameters();
    
    IMethodBuilderStage4<TParam1, TReturnType> WithParameter<TParam1>();
    IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Type param1);
    IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(string param1);
    IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Func<Type> param1Factory);
    IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Func<string> param1Factory);
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
    IMethodBodyGenerator UseProvidedBody(Action body);
}

public interface IMethodBuilderStage4ReturnVoid<TParam1>
{
    IMethodBodyGenerator UseProvidedBody(Action<TParam1> body);
}

public interface IMethodBuilderStage4ReturnVoid
{
    IMethodBodyGenerator UseProvidedBody(Action<object> body);
}

public interface IMethodBuilderStage4NoParam<TReturnType>
{
    IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory);
}

public interface IMethodBuilderStage4<TParam1, TReturnType>
{
    IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory);
    IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> BodyWithSwitchStatement();
}

public interface IMethodBuilderStage4WithSomeParam<TReturnType>
{
    IMethodBodyGenerator UseProvidedBody(Func<object, TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory);
}

public interface IMethodBuilderStage4WithSomeReturnTypeNoParam
{
    IMethodBodyGenerator UseProvidedBody(Func<object> body);
    IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory);
}

public interface IMethodBuilderStage4WithSomeReturnType<TParam1>
{
    IMethodBodyGenerator UseProvidedBody(Func<TParam1, object> body);
    IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory);
}

public interface IMethodBuilderStage4WithSomeReturnTypeWithSomeParam
{
    IMethodBodyGenerator UseProvidedBody(Func<object, object> body);
    IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory);
}