namespace MattSourceGenHelpers.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwitchCase(
    object? arg1 = null,
    object? arg2 = null,
    object? arg3 = null,
    object? arg4 = null,
    object? arg5 = null,
    object? arg6 = null,
    object? arg7 = null,
    object? arg8 = null,
    object? arg9 = null,
    object? arg10 = null
    ) 
    : Attribute
{
    
}