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
    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });

    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory) =>
        new DataMethodBodyGenerator(Data with { ReturnConstantValueFactory = constantValueFactory });

    public IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> BodyWithSwitchStatement()
        => throw new NotImplementedException(); //TODO: Remove explicit SwitchStatements with `for` and `constants`, like so:
    /*
        .ForMethod().WithReturnType<int>().WithParameter<int>()
        .BodyWithSwitchStatement()
        .ForCases(0, 1, 2, 300, 301, 302, 303).ReturnConstantValue(decimalNumber => SlowMath.CalculatePiDecimal(decimalNumber))
        .ForDefaultCase().UseProvidedBody(decimalNumber => SlowMath.CalculatePiDecimal(decimalNumber));

        Will be replaced with:

         .ForMethod().WithReturnType<int>().WithParameter<int>()
         .WithCompileTimeConstants(() => new
             {
                PrecomputedTargets = new HashSet<int>(new int[] { 0, 1, 2, 300, 301, 302, 303 })
             })
         .UseProvidedBody(decimalNumber, constants =>
            {
                if (constants.PrecomputedTargets.Contains(decimalNumber)) return SlowMath.CalculatePiDecimal(decimalNumber);
                else return SlowMath.CalculatePiDecimal(decimalNumber);
            });

        Or (in the case of PI), just simply:

        .ForMethod().WithReturnType<int>().WithParameter<int>()
        .UseProvidedBody(decimalNumber =>
            {
                var precomputedTargets = new HashSet<int>(new int[] { 0, 1, 2, 300, 301, 302, 303 })

                if (precomputedTargets.Contains(decimalNumber)) return SlowMath.CalculatePiDecimal(decimalNumber);
                else return SlowMath.CalculatePiDecimal(decimalNumber);
            });
     */
}

public record DataMethodBodyBuilderStage4NoArg<TReturnType>(BodyGenerationData Data) : IMethodBodyBuilderStage4NoArg<TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body) => new DataMethodBodyGenerator(Data with { RuntimeDelegateBody = body });

    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory) =>
        new DataMethodBodyGenerator(Data with { ReturnConstantValueFactory = constantValueFactory });
}

public record DataMethodBodyBuilderStage4ReturnVoid<TParam1>(BodyGenerationData BodyGenerationData) : IMethodBodyBuilderStage4ReturnVoid<TParam1>
{
    public IMethodBodyGenerator UseProvidedBody(Action<TParam1> body) => new DataMethodBodyGenerator(BodyGenerationData with { RuntimeDelegateBody = body });
}

public record DataMethodBodyBuilderStage4ReturnVoidNoArg(BodyGenerationData BodyGenerationData) : IMethodBodyBuilderStage4ReturnVoidNoArg
{
    public IMethodBodyGenerator UseProvidedBody(Action body) => new DataMethodBodyGenerator(BodyGenerationData with { RuntimeDelegateBody = body });
}