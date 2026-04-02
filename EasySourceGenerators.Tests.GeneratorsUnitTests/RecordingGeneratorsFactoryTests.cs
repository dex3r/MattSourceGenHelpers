using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators.DataBuilding;

namespace EasySourceGenerators.Tests.Generators;

[TestFixture]
public class DataGeneratorsFactoryTests
{
    [Test]
    public void StartFluentApiBuilderForBody_ReturnsDataMethodBodyBuilderStage1()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        IMethodBodyBuilderStage1 result = factory.StartFluentApiBuilderForBody();

        Assert.That(result, Is.TypeOf<DataMethodBodyBuilderStage1>());
    }

    [Test]
    public void StartFluentApiBuilderForBody_InitializesEmptyBodyGenerationData()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        IMethodBodyBuilderStage1 result = factory.StartFluentApiBuilderForBody();

        DataMethodBodyBuilderStage1 stage1 = (DataMethodBodyBuilderStage1)result;
        Assert.That(stage1.Data.ReturnType, Is.Null);
        Assert.That(stage1.Data.ParametersTypes, Is.Null);
        Assert.That(stage1.Data.RuntimeDelegateBody, Is.Null);
        Assert.That(stage1.Data.ReturnConstantValueFactory, Is.Null);
    }

    [Test]
    public void StartFluentApiBuilderForMethod_ThrowsNotImplementedException()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        Assert.Throws<NotImplementedException>(() => factory.StartFluentApiBuilderForMethod(() => "test"));
    }

    [Test]
    public void FullChain_ReturnsDataMethodBodyGenerator()
    {
        DataGeneratorsFactory factory = new DataGeneratorsFactory();

        IMethodBodyGenerator result = factory.StartFluentApiBuilderForBody()
            .ForMethod()
            .WithReturnType<int>()
            .WithNoParameters()
            .BodyReturningConstant(() => 42);

        Assert.That(result, Is.TypeOf<DataMethodBodyGenerator>());
    }
}
