using System;
using EasySourceGenerators.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace EasySourceGenerators.Tests;

[TestFixture]
public class DefaultCaseThrowExpressionTests
{
    [Test]
    public void SwitchDefaultThrowExpression_ThrowsExpectedExceptionAtRuntime()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => DefaultCaseThrowExpressionClass.Foo(123))!;
        Assert.That(exception.Message, Is.EqualTo("Unsupported input"));
    }

    [Test]
    public void SwitchDefaultThrowExpression_ProducesThrowDefaultClauseInGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DefaultCaseThrowExpressionClass_Foo.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              static partial class DefaultCaseThrowExpressionClass
                              {
                                  public static partial int Foo(int input)
                                  {
                                      switch (input)
                                      {
                                          default: throw new InvalidOperationException("Unsupported input");
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class DefaultCaseThrowExpressionClass
{
    public static partial int Foo(int input);

    [GeneratesMethod(nameof(Foo))]
    [SwitchDefault]
    private static Func<int, int> Foo_Generator_Default() => _ => throw new InvalidOperationException("Unsupported input");
}
