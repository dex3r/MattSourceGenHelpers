namespace EasySourceGenerators.Generators;

/// <summary>
/// Base type for method body data. Generators should build source code based on this data alone,
/// regardless of how it was constructed (fluent API, attributes, etc.).
/// </summary>
internal abstract record DataMethodBody;

/// <summary>
/// A method body that returns a single constant value.
/// </summary>
internal sealed record DataSimpleReturnBody(string? ReturnValue) : DataMethodBody;

/// <summary>
/// A method body that uses a switch statement.
/// </summary>
internal sealed record DataSwitchBody(
    IReadOnlyList<DataSwitchCase> Cases,
    DataSwitchDefaultCase? DefaultCase) : DataMethodBody;

/// <summary>
/// A single case in a switch body, with a key and a formatted C# literal value.
/// </summary>
internal sealed record DataSwitchCase(object Key, string FormattedValue);

/// <summary>
/// The default case in a switch body, with an expression to use (e.g., return value or throw).
/// </summary>
internal sealed record DataSwitchDefaultCase(string Expression);
