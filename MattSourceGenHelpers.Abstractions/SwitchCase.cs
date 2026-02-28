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
    public object? Arg1 => arg1;
    public object? Arg2 => arg2;
    public object? Arg3 => arg3;
    public object? Arg4 => arg4;
    public object? Arg5 => arg5;
    public object? Arg6 => arg6;
    public object? Arg7 => arg7;
    public object? Arg8 => arg8;
    public object? Arg9 => arg9;
    public object? Arg10 => arg10;
}
