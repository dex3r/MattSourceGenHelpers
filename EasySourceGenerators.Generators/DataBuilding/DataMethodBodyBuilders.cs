using System;
using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Generators.DataBuilding;

public record DataMethodBodyGenerator(BodyGenerationData Data) : IMethodBodyGenerator;

public record DataMethodBodyBuilderStage1(BodyGenerationData Data) : IMethodBodyBuilderStage1
{
    public IMethodBodyBuilderStage2 ForMethod() => new DataMethodBodyBuilderStage2(Data);
}

public record DataMethodBodyBuilderStage2(BodyGenerationData Data) : IMethodBodyBuilderStage2
{
    public IMethodBodyBuilderStage3ReturnVoid WithVoidReturnType() => new DataMethodBodyBuilderStage3ReturnVoid(Data with { ReturnType = typeof(void) });

    public IMethodBodyBuilderStage3<TReturnType> WithReturnType<TReturnType>() =>
        new DataMethodBodyBuilderStage3<TReturnType>(Data with { ReturnType = typeof(TReturnType) });
}

public record DataMethodBodyBuilderStage3ReturnVoid(BodyGenerationData Data) : IMethodBodyBuilderStage3ReturnVoid
{
    public IMethodBodyBuilderStage4ReturnVoidNoArg WithNoParameters() =>
        new DataMethodBodyBuilderStage4ReturnVoidNoArg(Data with { ParametersTypes = [] });

    public IMethodBodyBuilderStage4ReturnVoid<TParam1> WithParameter<TParam1>() =>
        new DataMethodBodyBuilderStage4ReturnVoid<TParam1>(Data with { ParametersTypes = [typeof(TParam1)] });
}

public record DataMethodBodyBuilderStage3<T>(BodyGenerationData Data) : IMethodBodyBuilderStage3<T>
{
    public IMethodBodyBuilderStage4NoArg<T> WithNoParameters() =>
        new DataMethodBodyBuilderStage4NoArg<T>(Data with { ParametersTypes = [] });

    public IMethodBodyBuilderStage4<TParam1, T> WithParameter<TParam1>() =>
        new DataMethodBodyBuilderStage4<TParam1, T>(Data with { ParametersTypes = [typeof(TParam1)] });
}

public record DataMethodBodyBuilderStage4<TParam1, TReturnType>(BodyGenerationData Data) : IMethodBodyBuilderStage4<TParam1, TReturnType>
{
    public IMethodBodyBuilderStage5WithConstants<TParam1, TReturnType, TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory) =>
        new DataMethodBodyBuilderStage5WithConstants<TParam1, TReturnType, TConstants>(Data with { CompileTimeConstants = compileTimeConstantsFactory() });

    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });

    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory) =>
        new DataMethodBodyGenerator(Data with { ReturnConstantValueFactory = constantValueFactory });
}

public record DataMethodBodyBuilderStage4NoArg<TReturnType>(BodyGenerationData Data) : IMethodBodyBuilderStage4NoArg<TReturnType>
{
    public IMethodBodyBuilderStage5NoArgWithConstants<TReturnType, TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory) =>
        new DataMethodBodyBuilderStage5NoArgWithConstants<TReturnType, TConstants>(Data with { CompileTimeConstants = compileTimeConstantsFactory() });

    public IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });

    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory) =>
        new DataMethodBodyGenerator(Data with { ReturnConstantValueFactory = constantValueFactory });
}

public record DataMethodBodyBuilderStage4ReturnVoid<TParam1>(BodyGenerationData BodyGenerationData) : IMethodBodyBuilderStage4ReturnVoid<TParam1>
{
    public IMethodBodyBuilderStage5ReturnVoidWithConstants<TParam1, TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory) =>
        new DataMethodBodyBuilderStage5ReturnVoidWithConstants<TParam1, TConstants>(BodyGenerationData with { CompileTimeConstants = compileTimeConstantsFactory() });

    public IMethodBodyGenerator UseProvidedBody(Action<TParam1> body) => new DataMethodBodyGenerator(BodyGenerationData with { RuntimeDelegateBody = body });
}

public record DataMethodBodyBuilderStage4ReturnVoidNoArg(BodyGenerationData BodyGenerationData) : IMethodBodyBuilderStage4ReturnVoidNoArg
{
    public IMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<TConstants> WithCompileTimeConstants<TConstants>(Func<TConstants> compileTimeConstantsFactory) =>
        new DataMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<TConstants>(BodyGenerationData with { CompileTimeConstants = compileTimeConstantsFactory() });

    public IMethodBodyGenerator UseProvidedBody(Action body) => new DataMethodBodyGenerator(BodyGenerationData with { RuntimeDelegateBody = body });
}

public record DataMethodBodyBuilderStage5WithConstants<TParam1, TReturnType, TConstants>(BodyGenerationData Data) : IMethodBodyBuilderStage5WithConstants<TParam1, TReturnType, TConstants>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TConstants, TParam1, TReturnType> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });

    public IMethodBodyGenerator BodyReturningConstant(Func<TConstants, TReturnType> constantValueFactory) =>
        new DataMethodBodyGenerator(Data with { ReturnConstantValueFactory = constantValueFactory });
}

public record DataMethodBodyBuilderStage5NoArgWithConstants<TReturnType, TConstants>(BodyGenerationData Data) : IMethodBodyBuilderStage5NoArgWithConstants<TReturnType, TConstants>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TConstants, TReturnType> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });

    public IMethodBodyGenerator BodyReturningConstant(Func<TConstants, TReturnType> constantValueFactory) =>
        new DataMethodBodyGenerator(Data with { ReturnConstantValueFactory = constantValueFactory });
}

public record DataMethodBodyBuilderStage5ReturnVoidWithConstants<TParam1, TConstants>(BodyGenerationData Data) : IMethodBodyBuilderStage5ReturnVoidWithConstants<TParam1, TConstants>
{
    public IMethodBodyGenerator UseProvidedBody(Action<TConstants, TParam1> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });
}

public record DataMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<TConstants>(BodyGenerationData Data) : IMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<TConstants>
{
    public IMethodBodyGenerator UseProvidedBody(Action<TConstants> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });
}