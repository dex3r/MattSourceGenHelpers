using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class RecordingGeneratorsFactoryTests
{
    [Test]
    public void ForMethod_ReturnsMethodBuilder()
    {
        RecordingGeneratorsFactory factory = new RecordingGeneratorsFactory();

        IMethodBuilder result = factory.ForMethod();

        Assert.That(result, Is.TypeOf<MethodBuilder>());
    }

    [Test]
    public void CreateImplementationTyped_SetsLastRecordAndReturnsTypedGenerator()
    {
        RecordingGeneratorsFactory factory = new RecordingGeneratorsFactory();

        IMethodImplementationGenerator<string> result = factory.CreateImplementation<string>();

        Assert.That(factory.LastRecord, Is.Not.Null);
        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGeneratorTyped<string>>());
    }
    
    [Test]
    public void CreateImplementationNonGeneric_SetsLastRecordAndReturnsGenerator()
    {
        RecordingGeneratorsFactory factory = new RecordingGeneratorsFactory();

        IMethodImplementationGenerator result = factory.CreateImplementation();

        Assert.That(factory.LastRecord, Is.Not.Null);
        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGenerator>());
    }

    [Test]
    public void CreateImplementationWithArg_SetsLastRecordAndReturnsArgGenerator()
    {
        RecordingGeneratorsFactory factory = new RecordingGeneratorsFactory();

        IMethodImplementationGenerator<int, string> result = factory.CreateImplementation<int, string>();

        Assert.That(factory.LastRecord, Is.Not.Null);
        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void TypedUseBody_ReturnsSameInstance()
    {
        RecordingMethodImplementationGeneratorTyped<string> generator = new RecordingMethodImplementationGeneratorTyped<string>();

        IMethodImplementationGenerator result = generator.UseBody(() => "a");

        Assert.That(result, Is.SameAs(generator));
    }

    [Test]
    public void WithSwitchBody_ReturnsSwitchBodyGenerator()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGenerator<int, string> generator = new RecordingMethodImplementationGenerator<int, string>(record);

        IMethodImplementationGeneratorSwitchBody<int, string> result = generator.WithSwitchBody();

        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void ForCases_FlattensAndConvertsCases()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGeneratorSwitchBody<int, string> switchBody = new RecordingMethodImplementationGeneratorSwitchBody<int, string>(record);

        switchBody.ForCases(1, new[] { 2, 3 }, "4").ReturnConstantValue(value => $"v{value}");

        Assert.That(record.CaseKeys, Is.EqualTo(new object[] { 1, 2, 3, 4 }));
        Assert.That(record.CaseValues, Is.EqualTo(new object?[] { "v1", "v2", "v3", "v4" }));
    }

    [Test]
    public void ReturnConstantValue_RecordsCaseValuesAndReturnsNewSwitchBody()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGeneratorSwitchBody<int, int> switchBody = new RecordingMethodImplementationGeneratorSwitchBody<int, int>(record);
        IMethodImplementationGeneratorSwitchBodyCase<int, int> caseBuilder = switchBody.ForCases(10, 20);

        IMethodImplementationGeneratorSwitchBody<int, int> result = caseBuilder.ReturnConstantValue(value => value + 1);

        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGeneratorSwitchBody<int, int>>());
        Assert.That(record.CaseKeys, Is.EqualTo(new object[] { 10, 20 }));
        Assert.That(record.CaseValues, Is.EqualTo(new object?[] { 11, 21 }));
    }

    [Test]
    public void UseBody_RecordsCaseKeysWithNullValues()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGeneratorSwitchBody<int, int> switchBody = new RecordingMethodImplementationGeneratorSwitchBody<int, int>(record);
        IMethodImplementationGeneratorSwitchBodyCase<int, int> caseBuilder = switchBody.ForCases(5, 6);

        IMethodImplementationGeneratorSwitchBody<int, int> result = caseBuilder.UseBody(_ => _ => { });

        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGeneratorSwitchBody<int, int>>());
        Assert.That(record.CaseKeys, Is.EqualTo(new object[] { 5, 6 }));
        Assert.That(record.CaseValues, Is.EqualTo(new object?[] { null, null }));
    }

    [Test]
    public void ReturnConstantValue_WithNullCase_ThrowsInvalidOperationException()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGeneratorSwitchBody<string, int> switchBody = new RecordingMethodImplementationGeneratorSwitchBody<string, int>(record);
        IMethodImplementationGeneratorSwitchBodyCase<string, int> caseBuilder = switchBody.ForCases((object?)null!);

        TestDelegate action = () => caseBuilder.ReturnConstantValue(_ => 1);

        Assert.That(action, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void UseBody_WithNullCase_ThrowsInvalidOperationException()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGeneratorSwitchBody<string, int> switchBody = new RecordingMethodImplementationGeneratorSwitchBody<string, int>(record);
        IMethodImplementationGeneratorSwitchBodyCase<string, int> caseBuilder = switchBody.ForCases((object?)null!);

        TestDelegate action = () => caseBuilder.UseBody(_ => _ => { });

        Assert.That(action, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ForDefaultCase_ReturnConstantValue_SetsDefaultFlag()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGeneratorSwitchBody<int, int> switchBody = new RecordingMethodImplementationGeneratorSwitchBody<int, int>(record);
        IMethodImplementationGeneratorSwitchBodyDefaultCase<int, int> defaultCase = switchBody.ForDefaultCase();

        IMethodImplementationGenerator<int, int> result = defaultCase.ReturnConstantValue(value => value + 1);

        Assert.That(record.HasDefaultCase, Is.True);
        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGenerator<int, int>>());
    }

    [Test]
    public void ForDefaultCase_UseBody_SetsDefaultFlag()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        RecordingMethodImplementationGeneratorSwitchBody<int, int> switchBody = new RecordingMethodImplementationGeneratorSwitchBody<int, int>(record);
        IMethodImplementationGeneratorSwitchBodyDefaultCase<int, int> defaultCase = switchBody.ForDefaultCase();

        IMethodImplementationGenerator<int, int> result = defaultCase.UseBody(_ => () => 1);

        Assert.That(record.HasDefaultCase, Is.True);
        Assert.That(result, Is.TypeOf<RecordingMethodImplementationGenerator<int, int>>());
    }
}
