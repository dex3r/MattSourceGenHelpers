using MattSourceGenHelpers.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace MattSourceGenHelpers.Tests;

[TestFixture]
public class BoolSwitchKeyTests
{
    [TestCase(true, "Yes")]
    [TestCase(false, "No")]
    public void BoolSwitchKey_ProducesExpectedRuntimeOutput(bool flag, string expected)
    {
        string result = TestBoolSwitchClass.GetBoolLabel(flag);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void BoolSwitchKey_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestBoolSwitchClass_GetBoolLabel.g.cs");
        string expectedCode = """
                              namespace MattSourceGenHelpers.Tests;

                              static partial class TestBoolSwitchClass
                              {
                                  public static partial string GetBoolLabel(bool flag)
                                  {
                                      switch (flag)
                                      {
                                          case true: return "Yes";
                                          case false: return "No";
                                          default: return "Unknown";
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class TestBoolSwitchClass
{
    public static partial string GetBoolLabel(bool flag);

    [GeneratesMethod(nameof(GetBoolLabel))]
    [SwitchCase(arg1: true)]
    [SwitchCase(arg1: false)]
    static string GetBoolLabel_Generator(bool flag) => flag ? "Yes" : "No";

    [GeneratesMethod(nameof(GetBoolLabel))]
    [SwitchDefault]
    static Func<bool, string> GetBoolLabel_Default() => _ => "Unknown";
}
