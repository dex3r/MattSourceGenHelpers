namespace EasySourceGenerators.Abstractions;

// ReSharper disable TypeParameterCanBeVariant - not available for every overload, so not used for consistency

public interface IMethodBodyBuilderStage1
{
    IMethodBodyBuilderStage2 ForMethod();
}

public interface IMethodBodyBuilderStage2
{
    IMethodBodyBuilderStage3ReturnVoid WithVoidReturnType();
    IMethodBodyBuilderStage3<TReturnType> WithReturnType<TReturnType>();
}

public interface IMethodBodyBuilderStage3ReturnVoid
{
    IMethodBodyBuilderStage4ReturnVoidNoArg WithNoParameters();
    IMethodBodyBuilderStage4ReturnVoid<TParam1> WithParameter<TParam1>();
}

public interface IMethodBodyBuilderStage3<TReturnType>
{
    IMethodBodyBuilderStage4NoArg<TReturnType> WithNoParameters();
    IMethodBodyBuilderStage4<TParam1, TReturnType> WithParameter<TParam1>();
}

public interface IMethodBodyBuilderStage4ReturnVoidNoArg
{
    IMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory);
    IMethodBodyGenerator UseProvidedBody(Action body);
}

public interface IMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<TConstants>
{
    IMethodBodyGenerator UseProvidedBody(Action<TConstants> body);
}

public interface IMethodBodyBuilderStage4NoArg<TReturnType>
{
    IMethodBodyBuilderStage5NoArgWithConstants<TReturnType, TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory);
    IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory);
}

public interface IMethodBodyBuilderStage5NoArgWithConstants<TReturnType, TConstants>
{
    IMethodBodyGenerator UseProvidedBody(Func<TConstants, TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TConstants, TReturnType> constantValueFactory);
}

public interface IMethodBodyBuilderStage4ReturnVoid<TParam1>
{
    IMethodBodyBuilderStage5ReturnVoidWithConstants<TParam1, TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory);
    IMethodBodyGenerator UseProvidedBody(Action<TParam1> body);
}

public interface IMethodBodyBuilderStage5ReturnVoidWithConstants<TParam1, TConstants>
{
    IMethodBodyGenerator UseProvidedBody(Action<TConstants, TParam1> body);
}

public interface IMethodBodyBuilderStage4<TParam1, in TReturnType>
{
    IMethodBodyBuilderStage5WithConstants<TParam1, TReturnType, TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory);
    
    IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory);
}

public interface IMethodBodyBuilderStage5WithConstants<TParam1, in TReturnType, TConstants>
{
    IMethodBodyGenerator UseProvidedBody(Func<TConstants, TParam1, TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TConstants, TReturnType> constantValueFactory);
}