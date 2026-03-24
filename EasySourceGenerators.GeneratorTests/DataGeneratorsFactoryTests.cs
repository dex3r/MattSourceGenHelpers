using EasySourceGenerators.Generators;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class DataGeneratorsFactoryTests
{
    [Test]
    public void CreateSimpleReturnBody_WithValue_CreatesCorrectData()
    {
        DataSimpleReturnBody result = DataGeneratorsFactory.CreateSimpleReturnBody("hello");

        Assert.That(result.ReturnValue, Is.EqualTo("hello"));
    }

    [Test]
    public void CreateSimpleReturnBody_WithNull_CreatesDataWithNullValue()
    {
        DataSimpleReturnBody result = DataGeneratorsFactory.CreateSimpleReturnBody(null);

        Assert.That(result.ReturnValue, Is.Null);
    }

    [Test]
    public void CreateSwitchBodyFromFluentData_WithCasesAndDefault_CreatesCorrectData()
    {
        SwitchBodyData fluentData = new SwitchBodyData(
            CasePairs: new List<(object key, string value)> { (1, "2"), (2, "4"), (3, "6") },
            HasDefaultCase: true);

        DataSwitchBody result = DataGeneratorsFactory.CreateSwitchBodyFromFluentData(fluentData, "0");

        Assert.That(result.Cases, Has.Count.EqualTo(3));
        Assert.That(result.Cases[0].Key, Is.EqualTo(1));
        Assert.That(result.Cases[0].FormattedValue, Is.EqualTo("2"));
        Assert.That(result.Cases[1].Key, Is.EqualTo(2));
        Assert.That(result.Cases[1].FormattedValue, Is.EqualTo("4"));
        Assert.That(result.Cases[2].Key, Is.EqualTo(3));
        Assert.That(result.Cases[2].FormattedValue, Is.EqualTo("6"));
        Assert.That(result.DefaultCase, Is.Not.Null);
        Assert.That(result.DefaultCase!.Expression, Is.EqualTo("0"));
    }

    [Test]
    public void CreateSwitchBodyFromFluentData_WithoutDefault_CreatesDataWithNullDefaultCase()
    {
        SwitchBodyData fluentData = new SwitchBodyData(
            CasePairs: new List<(object key, string value)> { (1, "one") },
            HasDefaultCase: false);

        DataSwitchBody result = DataGeneratorsFactory.CreateSwitchBodyFromFluentData(fluentData, null);

        Assert.That(result.Cases, Has.Count.EqualTo(1));
        Assert.That(result.DefaultCase, Is.Null);
    }

    [Test]
    public void CreateSwitchBodyFromFluentData_WithEmptyCases_CreatesEmptyData()
    {
        SwitchBodyData fluentData = new SwitchBodyData(
            CasePairs: new List<(object key, string value)>(),
            HasDefaultCase: false);

        DataSwitchBody result = DataGeneratorsFactory.CreateSwitchBodyFromFluentData(fluentData, null);

        Assert.That(result.Cases, Is.Empty);
        Assert.That(result.DefaultCase, Is.Null);
    }

    [Test]
    public void CreateSwitchBodyFromFluentData_WithDefaultOnly_CreatesDataWithDefaultAndNoCases()
    {
        SwitchBodyData fluentData = new SwitchBodyData(
            CasePairs: new List<(object key, string value)>(),
            HasDefaultCase: true);

        DataSwitchBody result = DataGeneratorsFactory.CreateSwitchBodyFromFluentData(fluentData, "888");

        Assert.That(result.Cases, Is.Empty);
        Assert.That(result.DefaultCase, Is.Not.Null);
        Assert.That(result.DefaultCase!.Expression, Is.EqualTo("888"));
    }

    [Test]
    public void CreateSwitchBodyFromFluentData_WithThrowExpression_PreservesExpression()
    {
        SwitchBodyData fluentData = new SwitchBodyData(
            CasePairs: new List<(object key, string value)> { (1, "\"Dog\"") },
            HasDefaultCase: true);

        DataSwitchBody result = DataGeneratorsFactory.CreateSwitchBodyFromFluentData(
            fluentData,
            "throw new ArgumentException(\"Unknown\")");

        Assert.That(result.DefaultCase, Is.Not.Null);
        Assert.That(result.DefaultCase!.Expression, Is.EqualTo("throw new ArgumentException(\"Unknown\")"));
    }

    [Test]
    public void CreateSwitchBodyFromFluentData_PreservesKeyTypes()
    {
        SwitchBodyData fluentData = new SwitchBodyData(
            CasePairs: new List<(object key, string value)>
            {
                (true, "\"Yes\""),
                ("hello", "\"World\""),
                (42, "\"Answer\"")
            },
            HasDefaultCase: false);

        DataSwitchBody result = DataGeneratorsFactory.CreateSwitchBodyFromFluentData(fluentData, null);

        Assert.That(result.Cases[0].Key, Is.TypeOf<bool>());
        Assert.That(result.Cases[1].Key, Is.TypeOf<string>());
        Assert.That(result.Cases[2].Key, Is.TypeOf<int>());
    }
}
