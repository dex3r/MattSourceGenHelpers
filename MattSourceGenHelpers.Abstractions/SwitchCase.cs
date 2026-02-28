namespace MattSourceGenHelpers.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwitchCase(
    object? arg1 = null
    ) 
    : Attribute
{
    public object? Arg1 { get; } = arg1;
}
