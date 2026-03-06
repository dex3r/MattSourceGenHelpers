namespace EasySourceGenerators.Abstractions;

public static class Generate
{
    internal static IGeneratorsFactory CurrentGenerator { get; [UsedImplicitly] set; } = new MockGeneratorsFactory();

    public static IMethodBuilder Method() => CurrentGenerator.ForMethod();
}
