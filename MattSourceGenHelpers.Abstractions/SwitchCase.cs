namespace MattSourceGenHelpers.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwitchCase(
    object? arg1 = null
    ) 
    : Attribute
{
    
}