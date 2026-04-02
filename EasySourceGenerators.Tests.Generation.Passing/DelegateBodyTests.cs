using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Tests.Generation.Passing.Helpers;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantIfElseBlock
// ReSharper disable ConvertSwitchStatementToSwitchExpression

namespace EasySourceGenerators.Tests.Generation.Passing;

public class DelegateBodyTests
{
    [Test]
    public void JustReturnConstantTest()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DelegateBodyTestClass_PartialMethod.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests.Generation.Passing;

                              static partial class DelegateBodyTestClass
                              {
                                  public static partial int PartialMethod()
                                  {
                                      return 42;
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
    
    [Test]
    public void JustReturnConstantWithParamTest()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DelegateBodyTestClass_WithParam_PartialMethod.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests.Generation.Passing;

                              static partial class DelegateBodyTestClass_WithParam
                              {
                                  public static partial int PartialMethod(int someParam)
                                  {
                                      return 42;
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
    
    [Test]
    public void JustReturnConstantWithIfTest()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DelegateBodyTestClass_WithIf_PartialMethod.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests.Generation.Passing;

                              static partial class DelegateBodyTestClass_WithIf
                              {
                                  public static partial int PartialMethod(int someParam)
                                  {
                                      if (someParam > 0)
                                      {
                                          return 42;
                                      }
                                      else
                                      {
                                          return -1;
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
    
    [Test]
    public void JustReturnConstantWithSwitchTest()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DelegateBodyTestClass_WithSwitch_PartialMethod.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests.Generation.Passing;

                              static partial class DelegateBodyTestClass_WithSwitch
                              {
                                  public static partial int PartialMethod(int someParam)
                                  {
                                      switch (someParam)
                                      {
                                          case -1: return 6;
                                          case 0: return 7;
                                          case 1: return 8;
                                          default: return -1;
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
    
    [Test]
    public void JustReturnConstantWithComplexBodyTest()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("DelegateBodyTestClass_WithComplexBody_PartialMethod.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests.Generation.Passing;

                              static partial class DelegateBodyTestClass_WithComplexBody
                              {
                                  public static partial int PartialMethod(int someParam)
                                  {
                                      int interResult;
                                      
                                      switch (someParam)
                                      {
                                          case -1: interResult = 6; break;
                                          case 0: interResult = 7; break;
                                          case 1: interResult = 8; break;
                                          default: interResult = -1; break;
                                      }
                                      
                                      return interResult * (new StringBuilder().ToString().Length + 1);
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class DelegateBodyTestClass
{
    public static partial int PartialMethod();

    [MethodBodyGenerator(nameof(PartialMethod))]
    public static IMethodBodyGenerator JustReturnConstantGenerator() =>
        Generate.MethodBody()
            .ForMethod().WithReturnType<int>().WithNoParameters()
            .UseProvidedBody(() => 42);
}

public static partial class DelegateBodyTestClass_WithParam
{
    public static partial int PartialMethod(int someParam);

    [MethodBodyGenerator(nameof(PartialMethod))]
    public static IMethodBodyGenerator JustReturnConstantGenerator() =>
        Generate.MethodBody()
            .ForMethod().WithReturnType<int>().WithParameter<int>()
            .UseProvidedBody(_ => 42);
}

public static partial class DelegateBodyTestClass_WithIf
{
    public static partial int PartialMethod(int someParam);

    [MethodBodyGenerator(nameof(PartialMethod))]
    public static IMethodBodyGenerator JustReturnConstantGenerator() =>
        Generate.MethodBody()
            .ForMethod().WithReturnType<int>().WithParameter<int>()
            .UseProvidedBody(someParam =>
            {
                if (someParam > 0)
                {
                    return 42;
                }
                else
                {
                    return -1;
                }
            });
}

public static partial class DelegateBodyTestClass_WithSwitch
{
    public static partial int PartialMethod(int someParam);

    [MethodBodyGenerator(nameof(PartialMethod))]
    public static IMethodBodyGenerator JustReturnConstantGenerator() =>
        Generate.MethodBody()
            .ForMethod().WithReturnType<int>().WithParameter<int>()
            .UseProvidedBody(someParam =>
            {
                switch (someParam)
                {
                    case -1: return 6;
                    case 0: return 7;
                    case 1: return 8;
                    default: return -1;
                }
            });
}

public static partial class DelegateBodyTestClass_WithComplexBody
{
    public static partial int PartialMethod(int someParam);

    [MethodBodyGenerator(nameof(PartialMethod))]
    public static IMethodBodyGenerator JustReturnConstantGenerator() =>
        Generate.MethodBody()
            .ForMethod().WithReturnType<int>().WithParameter<int>()
            .UseProvidedBody(someParam =>
            {
                int interResult;
                
                switch (someParam)
                {
                    case -1: interResult = 6; break;
                    case 0: interResult = 7; break;
                    case 1: interResult = 8; break;
                    default: interResult = -1; break;
                }

                return interResult * (new StringBuilder().ToString().Length + 1);
            });
}