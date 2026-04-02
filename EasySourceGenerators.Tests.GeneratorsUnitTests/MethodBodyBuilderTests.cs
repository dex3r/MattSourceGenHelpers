using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators.DataBuilding;

namespace EasySourceGenerators.Tests.Generators;

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
}
