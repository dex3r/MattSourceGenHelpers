using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

/// <summary>
/// Marks a static method as a source generator to create body for partial method.
/// </summary>
/// <param name="sameClassMethodName">Name of a partial method to generate body for.
/// Must be declared in the same class as method marked with this attribute.
/// Can be static or non-static.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
public class MethodBodyGenerator(string sameClassMethodName) : Attribute
{
    public string SameClassMethodName { get; } = sameClassMethodName;
}