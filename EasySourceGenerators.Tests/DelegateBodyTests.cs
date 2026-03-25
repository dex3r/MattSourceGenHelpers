using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Tests;

public class DelegateBodyTests
{
    [Test]
    public void JustReturnConstantTest()
    {
        
    }
}

public static partial class JustReturnConstantTestClass
{
    public static partial int JustReturnConstant();

    [MethodBodyGenerator(nameof(JustReturnConstant))]
    public static int JustReturnConstantGenerator() => 2;
}