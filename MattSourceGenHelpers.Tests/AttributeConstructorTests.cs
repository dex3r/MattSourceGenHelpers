using MattSourceGenHelpers.Abstractions;

namespace MattSourceGenHelpers.Tests;

[TestFixture]
public class AttributeConstructorTests
{
    [Test]
    public void GeneratesMethod_ExposesProvidedMethodName()
    {
        GeneratesMethod attribute = new(nameof(PiExampleTests));

        Assert.That(attribute.SameClassMethodName, Is.EqualTo(nameof(PiExampleTests)));
    }

    [Test]
    public void SwitchCase_ExposesProvidedArguments()
    {
        SwitchCase attribute = new(arg1: 1, arg2: "two", arg10: true);

        Assert.Multiple(() =>
        {
            Assert.That(attribute.Arg1, Is.EqualTo(1));
            Assert.That(attribute.Arg2, Is.EqualTo("two"));
            Assert.That(attribute.Arg10, Is.EqualTo(true));
        });
    }
}
