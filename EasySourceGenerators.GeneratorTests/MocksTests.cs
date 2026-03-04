using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class MocksTests
{
    [Test]
    public void MockGeneratorsFactory_ForMethod_ReturnsMockMethodBuilder()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodBuilder result = factory.ForMethod();

        Assert.That(result, Is.TypeOf<MockMethodBuilder>());
    }

    [Test]
    public void MockGeneratorsFactory_CreateImplementationTyped_ReturnsTypedMock()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodImplementationGenerator<string> result = factory.CreateImplementation<string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<string>>());
    }

    [Test]
    public void MockGeneratorsFactory_CreateImplementationWithArg_ReturnsArgMock()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodImplementationGenerator<int, string> result = factory.CreateImplementation<int, string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void MockMethodImplementationGeneratorTyped_UseBody_ReturnsSameInstance()
    {
        MockMethodImplementationGenerator<string> generator = new MockMethodImplementationGenerator<string>();

        IMethodImplementationGenerator result = generator.UseBody(() => "x");

        Assert.That(result, Is.SameAs(generator));
    }

    [Test]
    public void MockMethodImplementationGeneratorWithArg_WithSwitchBody_ReturnsSwitchBodyMock()
    {
        MockMethodImplementationGenerator<int, string> generator = new MockMethodImplementationGenerator<int, string>();

        IMethodImplementationGeneratorSwitchBody<int, string> result = generator.WithSwitchBody();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockSwitchBody_ForCases_ReturnsCaseMock()
    {
        MockMethodImplementationGeneratorSwitchBody<int, string> switchBody = new MockMethodImplementationGeneratorSwitchBody<int, string>();

        IMethodImplementationGeneratorSwitchBodyCase<int, string> result = switchBody.ForCases(1, 2);

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBodyCase<int, string>>());
    }

    [Test]
    public void MockSwitchBody_ForDefaultCase_ReturnsDefaultCaseMock()
    {
        MockMethodImplementationGeneratorSwitchBody<int, string> switchBody = new MockMethodImplementationGeneratorSwitchBody<int, string>();

        IMethodImplementationGeneratorSwitchBodyDefaultCase<int, string> result = switchBody.ForDefaultCase();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>>());
    }

    [Test]
    public void MockDefaultCase_ReturnConstantValue_ReturnsMethodImplementationGenerator()
    {
        MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string> defaultCase = new MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>();

        IMethodImplementationGenerator<int, string> result = defaultCase.ReturnConstantValue(value => value.ToString());

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void MockDefaultCase_UseBody_ReturnsMethodImplementationGenerator()
    {
        MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string> defaultCase = new MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>();

        IMethodImplementationGenerator<int, string> result = defaultCase.UseBody(_ => () => "v");

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void MockCase_ReturnConstantValue_ReturnsSwitchBody()
    {
        MockMethodImplementationGeneratorSwitchBodyCase<int, string> caseBuilder = new MockMethodImplementationGeneratorSwitchBodyCase<int, string>();

        IMethodImplementationGeneratorSwitchBody<int, string> result = caseBuilder.ReturnConstantValue(value => value.ToString());

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockCase_UseBody_ReturnsSwitchBody()
    {
        MockMethodImplementationGeneratorSwitchBodyCase<int, string> caseBuilder = new MockMethodImplementationGeneratorSwitchBodyCase<int, string>();

        IMethodImplementationGeneratorSwitchBody<int, string> result = caseBuilder.UseBody(_ => _ => { });

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockMethodBuilder_WithParameter_ReturnsGenericBuilder()
    {
        MockMethodBuilder builder = new MockMethodBuilder();

        IMethodBuilder<int> result = builder.WithParameter<int>();

        Assert.That(result, Is.TypeOf<MockMethodBuilder<int>>());
    }

    [Test]
    public void MockMethodBuilder_WithReturnType_ReturnsMockImplementationGenerator()
    {
        MockMethodBuilder builder = new MockMethodBuilder();

        IMethodImplementationGenerator<string> result = builder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MockImplementationGenerator<string>>());
    }

    [Test]
    public void MockImplementationGenerator_UseBody_ReturnsSameInstance()
    {
        MockImplementationGenerator<string> generator = new MockImplementationGenerator<string>();

        IMethodImplementationGenerator result = generator.UseBody(() => "x");

        Assert.That(result, Is.SameAs(generator));
    }

    [Test]
    public void MockMethodBuilderGeneric_WithReturnType_ReturnsArgImplementationGenerator()
    {
        MockMethodBuilder<int> builder = new MockMethodBuilder<int>();

        IMethodImplementationGenerator<int, string> result = builder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }
}
