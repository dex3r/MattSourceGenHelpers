using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
public class MethodTemplate : Attribute;