using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Generators;

public class MethodBodyBuilder(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage1
{
    public IMethodBodyBuilderStage2 ForMethod() => new MethodBodyBuilderStage2(generatorsFactory);

    public IMethodBodyBuilder<TArg1> WithParameter<TArg1>() => new MethodBodyBodyBuilder<TArg1>(generatorsFactory);

    public IMethodBodyGenerator<TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TReturnType>();
}

public class MethodBodyBuilderStage2(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage2
{
    public IMethodBodyBuilderStage3ReturnVoid WithVoidReturnType() => new MethodBodyBuilderStage3ReturnVoid(generatorsFactory);

    public IMethodBodyBuilderStage3<TReturnType> WithReturnType<TReturnType>() => new MethodBodyBuilderStage3<TReturnType>(generatorsFactory);
}

public class MethodBodyBuilderStage3ReturnVoid(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage3ReturnVoid
{
    public IMethodBodyBuilderStage4ReturnVoidNoArg WithNoParameters() => new MethodBodyBuilderStage4ReturnVoidNoArg(generatorsFactory);

    public IMethodBodyBuilderStage4ReturnVoid<TParam1> WithOneParameter<TParam1>() => new MethodBodyBuilderStage4ReturnVoid<TParam1>(generatorsFactory);
}

public class MethodBodyBuilderStage3<TReturnType>(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage3<TReturnType>
{
    public IMethodBodyBuilderStage4NoArg<TReturnType> WithNoParameters() => new MethodBodyBuilderStage4NoArg<TReturnType>(generatorsFactory);

    public IMethodBodyBuilderStage4<TParam1, TReturnType> WithOneParameter<TParam1>() =>
        new MethodBodyBuilderStage4<TParam1, TReturnType>(generatorsFactory);
}

public class MethodBodyBuilderStage4ReturnVoidNoArg(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage4ReturnVoidNoArg
{
    public IMethodBodyGenerator UseProvidedBody(Action body)
    {
        body();
        return generatorsFactory.CreateImplementation();
    }
}

public class MethodBodyBuilderStage4ReturnVoid<TParam1>(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage4ReturnVoid<TParam1>
{
    public IMethodBodyGenerator UseProvidedBody(Action<TParam1> body) => generatorsFactory.CreateImplementation();
}

public class MethodBodyBuilderStage4NoArg<TReturnType>(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage4NoArg<TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body) => generatorsFactory.CreateImplementation();

    public IMethodBodyGenerator BodyRetuningConstant(Func<TReturnType> constantValueFactory) =>
        generatorsFactory.CreateImplementation<TReturnType>();
}

public class MethodBodyBuilderStage4<TParam1, TReturnType>(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilderStage4<TParam1, TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body) =>
        generatorsFactory.CreateImplementation<TParam1, TReturnType>();

    public IMethodBodyGenerator BodyRetuningConstant(Func<TReturnType> constantValueFactory) =>
        generatorsFactory.CreateImplementation<TParam1, TReturnType>();

    public IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> BodyWithSwitchStatement() =>
        generatorsFactory.CreateImplementation<TParam1, TReturnType>().GenerateSwitchBody();
}

public class MethodBodyBodyBuilder<TArg1>(IMethodBodyGeneratorStage0 generatorsFactory) : IMethodBodyBuilder<TArg1>
{
    public IMethodBodyGenerator<TArg1, TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TArg1, TReturnType>();
}
