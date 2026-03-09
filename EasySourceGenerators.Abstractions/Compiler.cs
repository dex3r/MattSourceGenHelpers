namespace EasySourceGenerators.Abstractions;

public static class Compiler
{
    /// <summary>
    /// Runs <paramref name="compileTimeConstantFactory"/> at compile time and replaces entire invocation of this method with the result.
    /// </summary>
    /// <example>
    /// The following code:
    /// <code>int a = CalculateConstant(() => Math.Pi * 2);</code>
    /// will be replaced during compilation with the following code:
    /// <code>int a = 6.2831853071795862</code>
    /// </example>
    public static T CalculateConstant<T>(Func<T> compileTimeConstantFactory)
    {
        throw new NotImplementedException();
    }
}