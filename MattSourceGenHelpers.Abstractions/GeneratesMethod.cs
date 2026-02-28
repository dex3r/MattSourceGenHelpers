namespace MattSourceGenHelpers.Abstractions;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class GeneratesMethod(string sameClassMethodName) : Attribute
{
    public string SameClassMethodName { get; } = sameClassMethodName;
}