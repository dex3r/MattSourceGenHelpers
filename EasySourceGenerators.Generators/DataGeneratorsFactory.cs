using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators;

/// <summary>
/// Factory for building <see cref="DataMethodBody"/> instances from various sources.
/// This provides the bridge between different generation patterns (fluent API, attributes, simple)
/// and the unified data model consumed by <see cref="DataMethodBodyBuilders"/>.
/// </summary>
internal static class DataGeneratorsFactory
{
    /// <summary>
    /// Creates a <see cref="DataSimpleReturnBody"/> from a simple generator method execution result.
    /// </summary>
    internal static DataSimpleReturnBody CreateSimpleReturnBody(string? returnValue)
    {
        return new DataSimpleReturnBody(returnValue);
    }

    /// <summary>
    /// Creates a <see cref="DataSwitchBody"/> from a <see cref="SwitchBodyData"/> record
    /// (produced by <see cref="GeneratesMethodExecutionRuntime.ExecuteFluentGeneratorMethod"/>).
    /// </summary>
    internal static DataSwitchBody CreateSwitchBodyFromFluentData(
        SwitchBodyData switchBodyData,
        string? defaultExpression)
    {
        List<DataSwitchCase> cases = new();
        foreach ((object key, string value) in switchBodyData.CasePairs)
        {
            cases.Add(new DataSwitchCase(key, value));
        }

        DataSwitchDefaultCase? defaultCase = defaultExpression != null
            ? new DataSwitchDefaultCase(defaultExpression)
            : null;

        return new DataSwitchBody(cases, defaultCase);
    }

    // NOTE: Explicit [SwitchCase] attribute-based data building will be added in a future PR.
    // The SwitchCase attribute pattern will be replaced with a different approach.
    // See DataMethodBodyBuilders.cs for details on the planned data flow.
}
