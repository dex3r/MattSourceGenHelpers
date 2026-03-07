namespace EasySourceGenerators.GeneratorTests;

public class SimpleMethodWithParameterTests
{
    [Test]
    public void SimpleMethodWithParameterTests_Test()
    {
        string source = """
                        using EasySourceGenerators.Abstractions;

                        namespace TestNamespace;

                        public partial class SimpleMethodWithParameterClass
                        {
                            public partial int SimpleMethodWithParameter(int someIntParameter);

                            [GeneratesMethod(sameClassMethodName: nameof(SimpleMethodWithParameter))]
                            private static int SimpleMethodWithParameter_Generator(int someIntParameter)
                            {
                                return 5;
                            }
                        }
                        """;

        //TODO: This should not compile and throw error MSGH007
    }
}