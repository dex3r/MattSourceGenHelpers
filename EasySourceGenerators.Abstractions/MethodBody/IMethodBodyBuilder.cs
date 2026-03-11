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
    IMethodBodyGenerator UseProvidedBody(Action body);
}

public interface IMethodBodyBuilderStage4NoArg<TReturnType>
{
    IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body);
    IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory);
}

public interface IMethodBodyBuilderStage4ReturnVoid<TParam1>
{
    IMethodBodyGenerator UseProvidedBody(Action<TParam1> body);
}

public interface IMethodBodyBuilderStage4<TParam1, in TReturnType>
{
    IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body);
    IMethodBodyGenerator BodyRetuningConstant(Func<TReturnType> constantValueFactory);
    IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> BodyWithSwitchStatement();
}