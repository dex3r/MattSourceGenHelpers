using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class MethodBuilderTests
{
    [Test]
    public void WithParameter_ReturnsGenericMethodBuilder()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBuilder builder = new MethodBuilder(factory);

        IMethodBuilder<int> result = builder.WithParameter<int>();

        Assert.That(result, Is.TypeOf<MethodBuilder<int>>());
    }

    [Test]
    public void WithReturnType_OnNonGenericBuilder_UsesFactoryCreateImplementation()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBuilder builder = new MethodBuilder(factory);

        IMethodImplementationGenerator<string> result = builder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<TrackingTypedImplementationGenerator<string>>());
        Assert.That(factory.TypedCreateImplementationCalls, Is.EqualTo(1));
    }

    [Test]
    public void WithReturnType_OnGenericBuilder_UsesFactoryCreateImplementationWithArg()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBuilder<int> builder = new MethodBuilder<int>(factory);

        IMethodImplementationGenerator<int, string> result = builder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<TrackingArgImplementationGenerator<int, string>>());
        Assert.That(factory.ArgCreateImplementationCalls, Is.EqualTo(1));
    }

    private sealed class TrackingGeneratorsFactory : IGeneratorsFactory
    {
        public int TypedCreateImplementationCalls { get; private set; }
        public int ArgCreateImplementationCalls { get; private set; }

        public IMethodBuilder ForMethod() => new MethodBuilder(this);

        public IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>()
        {
            TypedCreateImplementationCalls++;
            return new TrackingTypedImplementationGenerator<TReturnType>();
        }

        public IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>()
        {
            ArgCreateImplementationCalls++;
            return new TrackingArgImplementationGenerator<TArg1, TReturnType>();
        }
    }

    private sealed class TrackingTypedImplementationGenerator<TReturnType> : IMethodImplementationGenerator<TReturnType>
    {
        public IMethodImplementationGenerator UseBody(Func<object> body) => this;
    }

    private sealed class TrackingArgImplementationGenerator<TArg1, TReturnType> : IMethodImplementationGenerator<TArg1, TReturnType>
    {
        public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> WithSwitchBody() =>
            new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
    }
}
