using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators.DataBuilding;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class DataMethodBodyBuilderTests
{
    [Test]
    public void ForMethod_ReturnsStage2()
    {
        DataMethodBodyBuilderStage1 stage1 = new DataMethodBodyBuilderStage1(new BodyGenerationData());

        IMethodBodyBuilderStage2 result = stage1.ForMethod();

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage2>());
    }

    [Test]
    public void WithReturnType_ReturnsStage3()
    {
        DataMethodBodyBuilderStage2 stage2 = new DataMethodBodyBuilderStage2(new BodyGenerationData());

        IMethodBodyBuilderStage3<string> result = stage2.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage3<string>>());
        Assert.That(((DataMethodBodyBuilderStage3<string>)result).Data.ReturnType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void WithVoidReturnType_ReturnsStage3ReturnVoid()
    {
        DataMethodBodyBuilderStage2 stage2 = new DataMethodBodyBuilderStage2(new BodyGenerationData());

        IMethodBodyBuilderStage3ReturnVoid result = stage2.WithVoidReturnType();

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage3ReturnVoid>());
        Assert.That(((DataMethodBodyBuilderStage3ReturnVoid)result).Data.ReturnType, Is.EqualTo(typeof(void)));
    }

    [Test]
    public void WithNoParameters_ReturnsStage4NoArg()
    {
        DataMethodBodyBuilderStage3<int> stage3 = new DataMethodBodyBuilderStage3<int>(new BodyGenerationData(ReturnType: typeof(int)));

        IMethodBodyBuilderStage4NoArg<int> result = stage3.WithNoParameters();

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage4NoArg<int>>());
        DataMethodBodyBuilderStage4NoArg<int> stage4 = (DataMethodBodyBuilderStage4NoArg<int>)result;
        Assert.That(stage4.Data.ParametersTypes, Is.Empty);
        Assert.That(stage4.Data.ReturnType, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void WithParameter_ReturnsStage4()
    {
        DataMethodBodyBuilderStage3<int> stage3 = new DataMethodBodyBuilderStage3<int>(new BodyGenerationData(ReturnType: typeof(int)));

        IMethodBodyBuilderStage4<string, int> result = stage3.WithParameter<string>();

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage4<string, int>>());
        DataMethodBodyBuilderStage4<string, int> stage4 = (DataMethodBodyBuilderStage4<string, int>)result;
        Assert.That(stage4.Data.ParametersTypes, Is.EqualTo(new[] { typeof(string) }));
    }

    [Test]
    public void BodyReturningConstant_SetsReturnConstantValueFactory()
    {
        DataMethodBodyBuilderStage4NoArg<int> stage4 = new DataMethodBodyBuilderStage4NoArg<int>(
            new BodyGenerationData(ReturnType: typeof(int), ParametersTypes: []));

        IMethodBodyGenerator result = stage4.BodyReturningConstant(() => 42);

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnConstantValueFactory, Is.Not.Null);
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Null);
    }

    [Test]
    public void UseProvidedBody_NoArg_SetsRuntimeDelegateBody()
    {
        DataMethodBodyBuilderStage4NoArg<string> stage4 = new DataMethodBodyBuilderStage4NoArg<string>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: []));

        IMethodBodyGenerator result = stage4.UseProvidedBody(() => "hello");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
        Assert.That(generator.Data.ReturnConstantValueFactory, Is.Null);
    }

    [Test]
    public void UseProvidedBody_WithArg_SetsRuntimeDelegateBody()
    {
        DataMethodBodyBuilderStage4<int, string> stage4 = new DataMethodBodyBuilderStage4<int, string>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: [typeof(int)]));

        IMethodBodyGenerator result = stage4.UseProvidedBody(x => x.ToString());

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
    }

    [Test]
    public void BodyReturningConstant_WithArg_SetsReturnConstantValueFactory()
    {
        DataMethodBodyBuilderStage4<int, string> stage4 = new DataMethodBodyBuilderStage4<int, string>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: [typeof(int)]));

        IMethodBodyGenerator result = stage4.BodyReturningConstant(() => "constant");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnConstantValueFactory, Is.Not.Null);
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Null);
    }

    [Test]
    public void VoidReturnType_WithNoParameters_UseProvidedBody_SetsCorrectData()
    {
        DataMethodBodyBuilderStage3ReturnVoid stage3 = new DataMethodBodyBuilderStage3ReturnVoid(
            new BodyGenerationData(ReturnType: typeof(void)));

        IMethodBodyBuilderStage4ReturnVoidNoArg stage4 = stage3.WithNoParameters();
        IMethodBodyGenerator result = stage4.UseProvidedBody(() => { });

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnType, Is.EqualTo(typeof(void)));
        Assert.That(generator.Data.ParametersTypes, Is.Empty);
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
    }

    [Test]
    public void VoidReturnType_WithParameter_UseProvidedBody_SetsCorrectData()
    {
        DataMethodBodyBuilderStage3ReturnVoid stage3 = new DataMethodBodyBuilderStage3ReturnVoid(
            new BodyGenerationData(ReturnType: typeof(void)));

        IMethodBodyBuilderStage4ReturnVoid<int> stage4 = stage3.WithParameter<int>();
        IMethodBodyGenerator result = stage4.UseProvidedBody(_ => { });

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnType, Is.EqualTo(typeof(void)));
        Assert.That(generator.Data.ParametersTypes, Is.EqualTo(new[] { typeof(int) }));
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
    }

    [Test]
    public void FullFluentChain_BodyReturningConstant_ProducesCorrectData()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        IMethodBodyGenerator result = factory.StartFluentApiBuilderForBody()
            .ForMethod()
            .WithReturnType<string>()
            .WithNoParameters()
            .BodyReturningConstant(() => "hello world");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnType, Is.EqualTo(typeof(string)));
        Assert.That(generator.Data.ParametersTypes, Is.Empty);
        Assert.That(generator.Data.ReturnConstantValueFactory, Is.Not.Null);
        object? constantValue = generator.Data.ReturnConstantValueFactory!.DynamicInvoke();
        Assert.That(constantValue, Is.EqualTo("hello world"));
    }

    [Test]
    public void FullFluentChain_UseProvidedBody_ProducesCorrectData()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        IMethodBodyGenerator result = factory.StartFluentApiBuilderForBody()
            .ForMethod()
            .WithReturnType<int>()
            .WithNoParameters()
            .UseProvidedBody(() => 42);

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnType, Is.EqualTo(typeof(int)));
        Assert.That(generator.Data.ParametersTypes, Is.Empty);
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
        object? bodyValue = generator.Data.RuntimeDelegateBody!.DynamicInvoke();
        Assert.That(bodyValue, Is.EqualTo(42));
    }

    [Test]
    public void WithCompileTimeConstants_WithParam_ReturnsStage5WithConstants()
    {
        DataMethodBodyBuilderStage4<int, string> stage4 = new DataMethodBodyBuilderStage4<int, string>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: [typeof(int)]));

        IMethodBodyBuilderStage5WithConstants<int, string, int> result = stage4.WithCompileTimeConstants(() => 42);

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage5WithConstants<int, string, int>>());
        DataMethodBodyBuilderStage5WithConstants<int, string, int> stage5 = (DataMethodBodyBuilderStage5WithConstants<int, string, int>)result;
        Assert.That(stage5.Data.CompileTimeConstants, Is.EqualTo(42));
    }

    [Test]
    public void WithCompileTimeConstants_NoArg_ReturnsStage5NoArgWithConstants()
    {
        DataMethodBodyBuilderStage4NoArg<string> stage4 = new DataMethodBodyBuilderStage4NoArg<string>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: []));

        IMethodBodyBuilderStage5NoArgWithConstants<string, int> result = stage4.WithCompileTimeConstants(() => 99);

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage5NoArgWithConstants<string, int>>());
        DataMethodBodyBuilderStage5NoArgWithConstants<string, int> stage5 = (DataMethodBodyBuilderStage5NoArgWithConstants<string, int>)result;
        Assert.That(stage5.Data.CompileTimeConstants, Is.EqualTo(99));
    }

    [Test]
    public void WithCompileTimeConstants_ReturnVoid_ReturnsStage5ReturnVoidWithConstants()
    {
        DataMethodBodyBuilderStage4ReturnVoid<int> stage4 = new DataMethodBodyBuilderStage4ReturnVoid<int>(
            new BodyGenerationData(ReturnType: typeof(void), ParametersTypes: [typeof(int)]));

        IMethodBodyBuilderStage5ReturnVoidWithConstants<int, string> result = stage4.WithCompileTimeConstants(() => "test");

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage5ReturnVoidWithConstants<int, string>>());
        DataMethodBodyBuilderStage5ReturnVoidWithConstants<int, string> stage5 = (DataMethodBodyBuilderStage5ReturnVoidWithConstants<int, string>)result;
        Assert.That(stage5.Data.CompileTimeConstants, Is.EqualTo("test"));
    }

    [Test]
    public void WithCompileTimeConstants_ReturnVoidNoArg_ReturnsStage5ReturnVoidNoArgWithConstants()
    {
        DataMethodBodyBuilderStage4ReturnVoidNoArg stage4 = new DataMethodBodyBuilderStage4ReturnVoidNoArg(
            new BodyGenerationData(ReturnType: typeof(void), ParametersTypes: []));

        IMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<int> result = stage4.WithCompileTimeConstants(() => 7);

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<int>>());
        DataMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<int> stage5 = (DataMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<int>)result;
        Assert.That(stage5.Data.CompileTimeConstants, Is.EqualTo(7));
    }

    [Test]
    public void Stage5WithConstants_UseProvidedBody_SetsRuntimeDelegateBody()
    {
        DataMethodBodyBuilderStage5WithConstants<int, string, int> stage5 = new DataMethodBodyBuilderStage5WithConstants<int, string, int>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: [typeof(int)], CompileTimeConstants: 42));

        IMethodBodyGenerator result = stage5.UseProvidedBody((constants, param) => $"{constants}_{param}");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
        Assert.That(generator.Data.CompileTimeConstants, Is.EqualTo(42));
    }

    [Test]
    public void Stage5WithConstants_BodyReturningConstant_SetsReturnConstantValueFactory()
    {
        DataMethodBodyBuilderStage5WithConstants<int, string, int> stage5 = new DataMethodBodyBuilderStage5WithConstants<int, string, int>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: [typeof(int)], CompileTimeConstants: 42));

        IMethodBodyGenerator result = stage5.BodyReturningConstant(constants => $"value_{constants}");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnConstantValueFactory, Is.Not.Null);
        Assert.That(generator.Data.CompileTimeConstants, Is.EqualTo(42));
    }

    [Test]
    public void Stage5NoArgWithConstants_UseProvidedBody_SetsRuntimeDelegateBody()
    {
        DataMethodBodyBuilderStage5NoArgWithConstants<string, int> stage5 = new DataMethodBodyBuilderStage5NoArgWithConstants<string, int>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: [], CompileTimeConstants: 10));

        IMethodBodyGenerator result = stage5.UseProvidedBody(constants => $"value_{constants}");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
        Assert.That(generator.Data.CompileTimeConstants, Is.EqualTo(10));
    }

    [Test]
    public void Stage5NoArgWithConstants_BodyReturningConstant_SetsReturnConstantValueFactory()
    {
        DataMethodBodyBuilderStage5NoArgWithConstants<string, int> stage5 = new DataMethodBodyBuilderStage5NoArgWithConstants<string, int>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: [], CompileTimeConstants: 10));

        IMethodBodyGenerator result = stage5.BodyReturningConstant(constants => $"const_{constants}");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnConstantValueFactory, Is.Not.Null);
        Assert.That(generator.Data.CompileTimeConstants, Is.EqualTo(10));
    }

    [Test]
    public void Stage5ReturnVoidWithConstants_UseProvidedBody_SetsRuntimeDelegateBody()
    {
        DataMethodBodyBuilderStage5ReturnVoidWithConstants<int, string> stage5 = new DataMethodBodyBuilderStage5ReturnVoidWithConstants<int, string>(
            new BodyGenerationData(ReturnType: typeof(void), ParametersTypes: [typeof(int)], CompileTimeConstants: "ctx"));

        IMethodBodyGenerator result = stage5.UseProvidedBody((constants, param) => { });

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
        Assert.That(generator.Data.CompileTimeConstants, Is.EqualTo("ctx"));
    }

    [Test]
    public void Stage5ReturnVoidNoArgWithConstants_UseProvidedBody_SetsRuntimeDelegateBody()
    {
        DataMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<string> stage5 = new DataMethodBodyBuilderStage5ReturnVoidNoArgWithConstants<string>(
            new BodyGenerationData(ReturnType: typeof(void), ParametersTypes: [], CompileTimeConstants: "ctx"));

        IMethodBodyGenerator result = stage5.UseProvidedBody(constants => { });

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
        Assert.That(generator.Data.CompileTimeConstants, Is.EqualTo("ctx"));
    }

    [Test]
    public void FullFluentChain_WithConstants_NoArg_BodyReturningConstant_ProducesCorrectData()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        IMethodBodyGenerator result = factory.StartFluentApiBuilderForBody()
            .ForMethod()
            .WithReturnType<string>()
            .WithNoParameters()
            .WithCompileTimeConstants(() => 42)
            .BodyReturningConstant(constants => $"value_{constants}");

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnType, Is.EqualTo(typeof(string)));
        Assert.That(generator.Data.ParametersTypes, Is.Empty);
        Assert.That(generator.Data.CompileTimeConstants, Is.EqualTo(42));
        Assert.That(generator.Data.ReturnConstantValueFactory, Is.Not.Null);
        object? constantValue = generator.Data.ReturnConstantValueFactory!.DynamicInvoke(42);
        Assert.That(constantValue, Is.EqualTo("value_42"));
    }

    [Test]
    public void FullFluentChain_WithConstants_WithParam_UseProvidedBody_ProducesCorrectData()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        IMethodBodyGenerator result = factory.StartFluentApiBuilderForBody()
            .ForMethod()
            .WithReturnType<int>()
            .WithParameter<int>()
            .WithCompileTimeConstants(() => new { Offset = 100 })
            .UseProvidedBody((constants, param) => constants.Offset + param);

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
        DataMethodBodyGenerator generator = (DataMethodBodyGenerator)result;
        Assert.That(generator.Data.ReturnType, Is.EqualTo(typeof(int)));
        Assert.That(generator.Data.ParametersTypes, Is.EqualTo(new[] { typeof(int) }));
        Assert.That(generator.Data.CompileTimeConstants, Is.Not.Null);
        Assert.That(generator.Data.RuntimeDelegateBody, Is.Not.Null);
    }

    [Test]
    public void WithCompileTimeConstants_FactoryIsInvokedImmediately()
    {
        int invocationCount = 0;
        DataMethodBodyBuilderStage4NoArg<string> stage4 = new DataMethodBodyBuilderStage4NoArg<string>(
            new BodyGenerationData(ReturnType: typeof(string), ParametersTypes: []));

        stage4.WithCompileTimeConstants(() =>
        {
            invocationCount++;
            return 42;
        });

        Assert.That(invocationCount, Is.EqualTo(1));
    }
}
