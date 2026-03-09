using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
public class MethodBodyGenerator(string sameClassMethodName) : Attribute
{
    public string SameClassMethodName { get; } = sameClassMethodName;
}