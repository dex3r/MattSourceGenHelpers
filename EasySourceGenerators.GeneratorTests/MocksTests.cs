// MocksTests have been commented out because the mock types (MockGeneratorsFactory, MockMethodBodyBuilder, etc.)
// were part of the old fluent API and have been replaced by the data abstraction layer
// (DataGeneratorsFactory, DataMethodBodyBuilders).

/*
using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class MocksTests
{
    [Test]
    public void MockGeneratorsFactory_ForMethod_ReturnsMockMethodBuilder()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodBodyBuilder result = factory.ForMethod();

        Assert.That(result, Is.TypeOf<MockMethodBodyBuilder>());
    }

    [Test]
    public void MockGeneratorsFactory_CreateImplementationTyped_ReturnsTypedMock()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodBodyGenerator<string> result = factory.CreateImplementation<string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<string>>());
    }

    [Test]
    public void MockGeneratorsFactory_CreateImplementationWithArg_ReturnsArgMock()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodBodyGenerator<int, string> result = factory.CreateImplementation<int, string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void MockMethodImplementationGeneratorTyped_UseBody_ReturnsSameInstance()
    {
        MockMethodImplementationGenerator<string> generator = new MockMethodImplementationGenerator<string>();

        IMethodBodyGeneratorWithNoParameter result = generator.BodyReturningConstantValue(() => "x");

        Assert.That(result, Is.SameAs(generator));
    }

    [Test]
    public void MockMethodImplementationGeneratorWithArg_WithSwitchBody_ReturnsSwitchBodyMock()
    {
        MockMethodImplementationGenerator<int, string> generator = new MockMethodImplementationGenerator<int, string>();

        IMethodBodyGeneratorSwitchBody<int, string> result = generator.GenerateSwitchBody();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockSwitchBody_ForCases_ReturnsCaseMock()
    {
        MockMethodImplementationGeneratorSwitchBody<int, string> switchBody = new MockMethodImplementationGeneratorSwitchBody<int, string>();

        IMethodBodyGeneratorSwitchBodyCase<int, string> result = switchBody.ForCases(1, 2);

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBodyCase<int, string>>());
    }

    [Test]
    public void MockSwitchBody_ForDefaultCase_ReturnsDefaultCaseMock()
    {
        MockMethodImplementationGeneratorSwitchBody<int, string> switchBody = new MockMethodImplementationGeneratorSwitchBody<int, string>();

        IMethodBodyGeneratorSwitchBodyDefaultCase<int, string> result = switchBody.ForDefaultCase();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>>());
    }

    [Test]
    public void MockDefaultCase_ReturnConstantValue_ReturnsMethodImplementationGenerator()
    {
        MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string> defaultCase = new MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>();

        IMethodBodyGenerator<int, string> result = defaultCase.ReturnConstantValue(value => value.ToString());

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void MockDefaultCase_UseBody_ReturnsMethodImplementationGenerator()
    {
        MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string> defaultCase = new MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>();

        IMethodBodyGenerator<int, string> result = defaultCase.UseProvidedBody(_ => () => "v");

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void MockCase_ReturnConstantValue_ReturnsSwitchBody()
    {
        MockMethodImplementationGeneratorSwitchBodyCase<int, string> caseBuilder = new MockMethodImplementationGeneratorSwitchBodyCase<int, string>();

        IMethodBodyGeneratorSwitchBody<int, string> result = caseBuilder.ReturnConstantValue(value => value.ToString());

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockCase_UseBody_ReturnsSwitchBody()
    {
        MockMethodImplementationGeneratorSwitchBodyCase<int, string> caseBuilder = new MockMethodImplementationGeneratorSwitchBodyCase<int, string>();

        IMethodBodyGeneratorSwitchBody<int, string> result = caseBuilder.UseBody(_ => _ => { });

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockMethodBuilder_WithParameter_ReturnsGenericBuilder()
    {
        MockMethodBodyBuilder bodyBuilder = new MockMethodBodyBuilder();

        IMethodBodyBuilder<int> result = bodyBuilder.WithParameter<int>();

        Assert.That(result, Is.TypeOf<MockMethodBodyBuilder<int>>());
    }

    [Test]
    public void MockMethodBuilder_WithReturnType_ReturnsMockImplementationGenerator()
    {
        MockMethodBodyBuilder bodyBuilder = new MockMethodBodyBuilder();

        IMethodBodyGenerator<string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MockImplementationGenerator<string>>());
    }

    [Test]
    public void MockImplementationGenerator_UseBody_ReturnsSameInstance()
    {
        MockImplementationGenerator<string> generator = new MockImplementationGenerator<string>();

        IMethodBodyGeneratorWithNoParameter result = generator.BodyReturningConstantValue(() => "x");

        Assert.That(result, Is.SameAs(generator));
    }

    [Test]
    public void MockMethodBuilderGeneric_WithReturnType_ReturnsArgImplementationGenerator()
    {
        MockMethodBodyBuilder<int> bodyBuilder = new MockMethodBodyBuilder<int>();

        IMethodBodyGenerator<int, string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }
}
*/
