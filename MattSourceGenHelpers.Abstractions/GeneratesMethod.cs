using JetBrains.Annotations;

namespace MattSourceGenHelpers.Abstractions;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse]
public class GeneratesMethod(string sameClassMethodName) : Attribute
{
    public string SameClassMethodName => sameClassMethodName;
}