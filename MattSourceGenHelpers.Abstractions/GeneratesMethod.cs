using JetBrains.Annotations;

namespace MattSourceGenHelpers.Abstractions;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
public class GeneratesMethod(string sameClassMethodName) : Attribute
{
    public string SameClassMethodName { get; } = sameClassMethodName;
}