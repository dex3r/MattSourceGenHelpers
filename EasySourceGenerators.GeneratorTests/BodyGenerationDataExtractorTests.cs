using EasySourceGenerators.Generators.DataBuilding;
using EasySourceGenerators.Generators.IncrementalGenerators;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class BodyGenerationDataExtractorTests
{
    [Test]
    public void Extract_WithCompileTimeConstants_BodyReturningConstant_InvokesWithConstants()
    {
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(
                ReturnType: typeof(string),
                ParametersTypes: [],
                CompileTimeConstants: 42,
                ReturnConstantValueFactory: (Func<int, string>)(constants => $"value_{constants}")));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, false);

        Assert.That(result.ReturnValue, Is.EqualTo("value_42"));
        Assert.That(result.IsVoid, Is.False);
    }

    [Test]
    public void Extract_WithCompileTimeConstants_RuntimeBodyNoArgs_InvokesWithConstants()
    {
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(
                ReturnType: typeof(string),
                ParametersTypes: [],
                CompileTimeConstants: 10,
                RuntimeDelegateBody: (Func<int, string>)(constants => $"body_{constants}")));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, false);

        Assert.That(result.ReturnValue, Is.EqualTo("body_10"));
        Assert.That(result.IsVoid, Is.False);
    }

    [Test]
    public void Extract_WithCompileTimeConstants_RuntimeBodyWithAdditionalParams_ReturnsNullValue()
    {
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(
                ReturnType: typeof(string),
                ParametersTypes: [typeof(int)],
                CompileTimeConstants: 10,
                RuntimeDelegateBody: (Func<int, int, string>)((constants, param) => $"{constants}_{param}")));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, false);

        Assert.That(result.ReturnValue, Is.Null);
        Assert.That(result.IsVoid, Is.False);
    }

    [Test]
    public void Extract_WithoutConstants_BodyReturningConstant_InvokesWithoutArgs()
    {
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(
                ReturnType: typeof(int),
                ParametersTypes: [],
                ReturnConstantValueFactory: (Func<int>)(() => 99)));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, false);

        Assert.That(result.ReturnValue, Is.EqualTo("99"));
    }

    [Test]
    public void Extract_WithoutConstants_RuntimeBodyNoParams_InvokesDirectly()
    {
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(
                ReturnType: typeof(string),
                ParametersTypes: [],
                RuntimeDelegateBody: (Func<string>)(() => "hello")));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, false);

        Assert.That(result.ReturnValue, Is.EqualTo("hello"));
    }

    [Test]
    public void Extract_VoidReturnType_WithConstants_RuntimeBody_InvokesWithConstants()
    {
        string captured = "";
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(
                ReturnType: typeof(void),
                ParametersTypes: [],
                CompileTimeConstants: "test",
                RuntimeDelegateBody: (Action<string>)(constants => { captured = constants; })));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, true);

        Assert.That(result.IsVoid, Is.True);
        Assert.That(captured, Is.EqualTo("test"));
    }

    [Test]
    public void Extract_NullBodyGenerationData_ReturnsNullReturnValue()
    {
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(ReturnType: typeof(string)));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, false);

        Assert.That(result.ReturnValue, Is.Null);
    }

    [Test]
    public void Extract_WithCompileTimeConstants_ConstantFactoryTakesPriority()
    {
        DataMethodBodyGenerator generator = new DataMethodBodyGenerator(
            new BodyGenerationData(
                ReturnType: typeof(string),
                ParametersTypes: [],
                CompileTimeConstants: 5,
                ReturnConstantValueFactory: (Func<int, string>)(constants => $"factory_{constants}"),
                RuntimeDelegateBody: (Func<int, string>)(constants => $"body_{constants}")));

        FluentBodyResult result = BodyGenerationDataExtractor.Extract(generator, false);

        Assert.That(result.ReturnValue, Is.EqualTo("factory_5"));
    }
}
