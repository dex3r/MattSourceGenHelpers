using System;
using System.Reflection;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Extracts <see cref="FluentBodyResult"/> data from a method result object via reflection.
/// The result object is expected to be a <c>DataMethodBodyGenerator</c> containing a
/// <c>BodyGenerationData</c> property, from which return values and delegate bodies are extracted.
/// </summary>
internal static class BodyGenerationDataExtractor
{
    /// <summary>
    /// Extracts the return value from a fluent body generator method result using reflection.
    /// Checks for <c>ReturnConstantValueFactory</c> first, then <c>RuntimeDelegateBody</c>.
    /// Returns a <see cref="FluentBodyResult"/> with the extracted value, or <c>null</c> return value
    /// if neither factory nor body are present.
    /// Sets <see cref="FluentBodyResult.HasDelegateBody"/> when <c>RuntimeDelegateBody</c> is present,
    /// indicating that the delegate body source code should be extracted from the syntax tree.
    /// </summary>
    internal static FluentBodyResult Extract(object methodResult, bool isVoidReturnType)
    {
        Type resultType = methodResult.GetType();

        // The result should be a DataMethodBodyGenerator containing a BodyGenerationData Data property
        PropertyInfo? dataProperty = resultType.GetProperty(Consts.BodyGenerationDataPropertyName);
        if (dataProperty == null)
        {
            // The method returned something that isn't a DataMethodBodyGenerator.
            // This may happen when the fluent chain is incomplete (e.g., user returned an intermediate builder).
            return new FluentBodyResult(null, isVoidReturnType, HasDelegateBody: false);
        }

        object? bodyGenerationData = dataProperty.GetValue(methodResult);
        if (bodyGenerationData == null)
        {
            return new FluentBodyResult(null, isVoidReturnType, HasDelegateBody: false);
        }

        Type dataType = bodyGenerationData.GetType();
        PropertyInfo? returnTypeProperty = dataType.GetProperty("ReturnType");
        Type? dataReturnType = returnTypeProperty?.GetValue(bodyGenerationData) as Type;
        bool isVoid = dataReturnType == typeof(void);

        bool hasDelegateBody = HasRuntimeDelegateBody(dataType, bodyGenerationData);

        return TryExtractFromConstantFactory(dataType, bodyGenerationData, isVoid)
               ?? TryExtractFromRuntimeBody(dataType, bodyGenerationData, isVoid, hasDelegateBody)
               ?? new FluentBodyResult(null, isVoid, hasDelegateBody);
    }

    /// <summary>
    /// Checks whether <c>RuntimeDelegateBody</c> is set (non-null) in the body generation data.
    /// </summary>
    private static bool HasRuntimeDelegateBody(Type dataType, object bodyGenerationData)
    {
        PropertyInfo? runtimeBodyProperty = dataType.GetProperty("RuntimeDelegateBody");
        Delegate? runtimeBody = runtimeBodyProperty?.GetValue(bodyGenerationData) as Delegate;
        return runtimeBody != null;
    }

    /// <summary>
    /// Attempts to extract a return value by invoking the <c>ReturnConstantValueFactory</c> delegate.
    /// </summary>
    private static FluentBodyResult? TryExtractFromConstantFactory(
        Type dataType,
        object bodyGenerationData,
        bool isVoid)
    {
        PropertyInfo? constantFactoryProperty = dataType.GetProperty("ReturnConstantValueFactory");
        Delegate? constantFactory = constantFactoryProperty?.GetValue(bodyGenerationData) as Delegate;
        if (constantFactory == null)
        {
            return null;
        }

        object? constantValue = constantFactory.DynamicInvoke();
        return new FluentBodyResult(constantValue?.ToString(), isVoid, HasDelegateBody: false);
    }

    /// <summary>
    /// Attempts to extract a return value by invoking the <c>RuntimeDelegateBody</c> delegate.
    /// Only invokes delegates with zero parameters; parameterized delegates cannot be executed
    /// at compile time without concrete values.
    /// </summary>
    private static FluentBodyResult? TryExtractFromRuntimeBody(
        Type dataType,
        object bodyGenerationData,
        bool isVoid,
        bool hasDelegateBody)
    {
        PropertyInfo? runtimeBodyProperty = dataType.GetProperty("RuntimeDelegateBody");
        Delegate? runtimeBody = runtimeBodyProperty?.GetValue(bodyGenerationData) as Delegate;
        if (runtimeBody == null)
        {
            return null;
        }

        ParameterInfo[] bodyParams = runtimeBody.Method.GetParameters();
        if (bodyParams.Length == 0)
        {
            object? bodyResult = runtimeBody.DynamicInvoke();
            return new FluentBodyResult(bodyResult?.ToString(), isVoid, hasDelegateBody);
        }

        // For delegates with parameters, we can't invoke at compile time without values
        return new FluentBodyResult(null, isVoid, hasDelegateBody);
    }
}
