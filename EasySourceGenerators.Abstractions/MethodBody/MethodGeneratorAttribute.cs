using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

/// <summary>
/// Marks a static method as a source generator to create a new method in this class. The new method be declared in the same class as method marked with
/// this attribute. Class has to be <see langword="partial"/>. Method has to return <see cref="IMethodGenerator"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
public class MethodGenerator : Attribute;