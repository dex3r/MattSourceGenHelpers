using JetBrains.Annotations;

namespace MattSourceGenHelpers.Abstractions;

[AttributeUsage(AttributeTargets.All)]
[MeansImplicitUse(ImplicitUseKindFlags.Access | ImplicitUseKindFlags.Access | ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
public class UsedBySourceGenerator : Attribute;