namespace EasySourceGenerators.Abstractions;

public interface IMethodBodyGenerator;

/// <summary>
/// <see cref="IMethodGenerator"/> implements <see cref="IMethodBodyGenerator"/> since fluent APIs that generate methods always returns
/// with method body generators.
/// </summary>
public interface IMethodGenerator : IMethodBodyGenerator;