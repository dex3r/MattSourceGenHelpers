using EasySourceGenerators.Abstractions.Method;

namespace EasySourceGenerators.Abstractions;

public class MockGeneratorsFactory : IGeneratorsFactory
{
    public IMethodBodyBuilderStage1 StartFluentApiBuilderForBody() => new MockMethodBodyBuilderStage1();

    public IMethodBuilderStage1 StartFluentApiBuilderForMethod() => new MockMethodBuilderStage1();

    public IMethodBodyBuilder ForMethod() => new MockMethodBodyBuilder();

    public IMethodBodyGenerator<TReturnType> CreateImplementation<TReturnType>() => new MockMethodImplementationGenerator<TReturnType>();

    public IMethodBodyGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>() =>
        new MockMethodImplementationGenerator<TArg1, TReturnType>();
}

public class MockMethodBodyBuilderStage1 : IMethodBodyBuilderStage1
{
    public IMethodBodyBuilderStage2 ForMethod() => new MockMethodBodyBuilderStage2();
    public IMethodBodyBuilder<TArg1> WithParameter<TArg1>() => new MockMethodBodyBuilder<TArg1>();
    public IMethodBodyGenerator<TReturnType> WithReturnType<TReturnType>() => new MockMethodImplementationGenerator<TReturnType>();
}

public class MockMethodBodyBuilderStage2 : IMethodBodyBuilderStage2
{
    public IMethodBodyBuilderStage3ReturnVoid WithVoidReturnType() => new MockMethodBodyBuilderStage3ReturnVoid();
    public IMethodBodyBuilderStage3<TReturnType> WithReturnType<TReturnType>() => new MockMethodBodyBuilderStage3<TReturnType>();
}

public class MockMethodBodyBuilderStage3ReturnVoid : IMethodBodyBuilderStage3ReturnVoid
{
    public IMethodBodyBuilderStage4ReturnVoidNoArg WithNoParameters() => new MockMethodBodyBuilderStage4ReturnVoidNoArg();
    public IMethodBodyBuilderStage4ReturnVoid<TParam1> WithOneParameter<TParam1>() => new MockMethodBodyBuilderStage4ReturnVoid<TParam1>();
}

public class MockMethodBodyBuilderStage3<TReturnType> : IMethodBodyBuilderStage3<TReturnType>
{
    public IMethodBodyBuilderStage4NoArg<TReturnType> WithNoParameters() => new MockMethodBodyBuilderStage4NoArg<TReturnType>();
    public IMethodBodyBuilderStage4<TParam1, TReturnType> WithOneParameter<TParam1>() => new MockMethodBodyBuilderStage4<TParam1, TReturnType>();
}

public class MockMethodBodyBuilderStage4ReturnVoidNoArg : IMethodBodyBuilderStage4ReturnVoidNoArg
{
    public IMethodBodyGenerator UseProvidedBody(Action body) => new MockMethodBodyGenerator();
}

public class MockMethodBodyBuilderStage4ReturnVoid<TParam1> : IMethodBodyBuilderStage4ReturnVoid<TParam1>
{
    public IMethodBodyGenerator UseProvidedBody(Action<TParam1> body) => new MockMethodBodyGenerator();
}

public class MockMethodBodyBuilderStage4NoArg<TReturnType> : IMethodBodyBuilderStage4NoArg<TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyRetuningConstant(Func<TReturnType> constantValueFactory) => new MockMethodBodyGenerator();
}

public class MockMethodBodyBuilderStage4<TParam1, TReturnType> : IMethodBodyBuilderStage4<TParam1, TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyRetuningConstant(Func<TReturnType> constantValueFactory) => new MockMethodBodyGenerator();
    public IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> BodyWithSwitchStatement() =>
        new MockMethodImplementationGeneratorSwitchBody<TParam1, TReturnType>();
}

public class MockMethodBodyGenerator : IMethodBodyGenerator;

public class MockMethodImplementationGenerator<TReturnType> : IMethodBodyGenerator<TReturnType>
{
    public IMethodBodyGeneratorWithNoParameter BodyReturningConstantValue(Func<object> body) => new MockMethodBodyGeneratorWithNoParameter();
}

public class MockMethodBodyGeneratorWithNoParameter : IMethodBodyGeneratorWithNoParameter;

public class MockMethodImplementationGenerator<TArg1, TReturnType> : IMethodBodyGenerator<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> GenerateSwitchBody() =>
        new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> : IMethodBodyGeneratorSwitchBody<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType> ForCases(params TArg1[] cases)
        => new MockMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>();

    public IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
        => new MockMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> : IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1,
    TReturnType>
{
    public IMethodBodyGenerator ReturnConstantValue(Func<TArg1, TReturnType> func)
        => new MockMethodBodyGenerator();

    public IMethodBodyGenerator UseProvidedBody(Func<TArg1, TReturnType> func)
        => new MockMethodBodyGenerator();
}

public class MockMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType> : IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyCaseStage2<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> constantValueFactory)
        => new MockMethodImplementationGeneratorSwitchBodyCaseStage2<TArg1, TReturnType>();

    public IMethodBodyGeneratorSwitchBodyCaseStage2<TArg1, TReturnType> UseProvidedBody(Func<TArg1, TReturnType> body)
        => new MockMethodImplementationGeneratorSwitchBodyCaseStage2<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBodyCaseStage2<TArg1, TReturnType> : IMethodBodyGeneratorSwitchBodyCaseStage2<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
        => new MockMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>();
}

public class MockMethodBodyBuilder : IMethodBodyBuilder
{
    public IMethodBodyBuilder<TArg1> WithParameter<TArg1>() => new MockMethodBodyBuilder<TArg1>();

    public IMethodBodyGenerator<TReturnType> WithReturnType<TReturnType>() => new MockMethodImplementationGenerator<TReturnType>();
}

public class MockMethodBodyBuilder<TArg1Input> : IMethodBodyBuilder<TArg1Input>
{
    public IMethodBodyGenerator<TArg1Input, TReturnType> WithReturnType<TReturnType>()
        => new MockMethodImplementationGenerator<TArg1Input, TReturnType>();
}

// Mock implementations for Method builder (entire method generation)
public class MockMethodBuilderStage1 : IMethodBuilderStage1
{
    public IMethodBuilderStage2 WithName(string name) => new MockMethodBuilderStage2();
    public IMethodBuilderStage2 WithName(Func<string> nameFactory) => new MockMethodBuilderStage2();
}

public class MockMethodBuilderStage2 : IMethodBuilderStage2
{
    public IMethodBuilderStage3ReturningVoid WithVoidReturn() => new MockMethodBuilderStage3ReturningVoid();
    public IMethodBuilderStage3<TReturnType> WithReturnType<TReturnType>() => new MockMethodBuilderStage3<TReturnType>();
    public IMethodBuilderStage3WithSomeReturnType WithReturnType(Type returnType) => new MockMethodBuilderStage3WithSomeReturnType();
    public IMethodBuilderStage3WithSomeReturnType WithReturnType(string returnType) => new MockMethodBuilderStage3WithSomeReturnType();
    public IMethodBuilderStage3WithSomeReturnType WithReturnType(Func<Type> returnTypeFactory) => new MockMethodBuilderStage3WithSomeReturnType();
    public IMethodBuilderStage3WithSomeReturnType WithReturnType(Func<string> returnTypeFactory) => new MockMethodBuilderStage3WithSomeReturnType();
}

public class MockMethodBuilderStage3ReturningVoid : IMethodBuilderStage3ReturningVoid
{
    public IMethodBuilderStage4ReturnVoidNoParam WithNoParameters() => new MockMethodBuilderStage4ReturnVoidNoParam();
    public IMethodBuilderStage4ReturnVoid<TParam1> WithParameter<TParam1>() => new MockMethodBuilderStage4ReturnVoid<TParam1>();
    public IMethodBuilderStage4ReturnVoid WithParameter(Type param1) => new MockMethodBuilderStage4ReturnVoidNonGeneric();
    public IMethodBuilderStage4ReturnVoid WithParameter(string param1) => new MockMethodBuilderStage4ReturnVoidNonGeneric();
    public IMethodBuilderStage4ReturnVoid WithParameter(Func<Type> param1Factory) => new MockMethodBuilderStage4ReturnVoidNonGeneric();
    public IMethodBuilderStage4ReturnVoid WithParameter(Func<string> param1Factory) => new MockMethodBuilderStage4ReturnVoidNonGeneric();
}

public class MockMethodBuilderStage3<TReturnType> : IMethodBuilderStage3<TReturnType>
{
    public IMethodBuilderStage4NoParam<TReturnType> WithNoParameters() => new MockMethodBuilderStage4NoParam<TReturnType>();
    public IMethodBuilderStage4<TParam1, TReturnType> WithParameter<TParam1>() => new MockMethodBuilderStage4<TParam1, TReturnType>();
    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Type param1) => new MockMethodBuilderStage4WithSomeParam<TReturnType>();
    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(string param1) => new MockMethodBuilderStage4WithSomeParam<TReturnType>();
    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Func<Type> param1Factory) => new MockMethodBuilderStage4WithSomeParam<TReturnType>();
    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Func<string> param1Factory) => new MockMethodBuilderStage4WithSomeParam<TReturnType>();
}

public class MockMethodBuilderStage3WithSomeReturnType : IMethodBuilderStage3WithSomeReturnType
{
    public IMethodBuilderStage4WithSomeReturnTypeNoParam WithNoParameters() => new MockMethodBuilderStage4WithSomeReturnTypeNoParam();
    public IMethodBuilderStage4WithSomeReturnType<TParam1> WithParameter<TParam1>() => new MockMethodBuilderStage4WithSomeReturnType<TParam1>();
    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Type param1) => new MockMethodBuilderStage4WithSomeReturnTypeWithSomeParam();
    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(string param1) => new MockMethodBuilderStage4WithSomeReturnTypeWithSomeParam();
    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Func<Type> param1Factory) => new MockMethodBuilderStage4WithSomeReturnTypeWithSomeParam();
    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Func<string> param1Factory) => new MockMethodBuilderStage4WithSomeReturnTypeWithSomeParam();
}

public class MockMethodBuilderStage4ReturnVoidNoParam : IMethodBuilderStage4ReturnVoidNoParam
{
    public IMethodBodyGenerator UseProvidedBody(Action body) => new MockMethodBodyGenerator();
}

public class MockMethodBuilderStage4ReturnVoid<TParam1> : IMethodBuilderStage4ReturnVoid<TParam1>
{
    public IMethodBodyGenerator UseProvidedBody(Action<TParam1> body) => new MockMethodBodyGenerator();
}

public class MockMethodBuilderStage4ReturnVoidNonGeneric : IMethodBuilderStage4ReturnVoid
{
    public IMethodBodyGenerator UseProvidedBody(Action<object> body) => new MockMethodBodyGenerator();
}

public class MockMethodBuilderStage4NoParam<TReturnType> : IMethodBuilderStage4NoParam<TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory) => new MockMethodBodyGenerator();
}

public class MockMethodBuilderStage4<TParam1, TReturnType> : IMethodBuilderStage4<TParam1, TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory) => new MockMethodBodyGenerator();
    public IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> BodyWithSwitchStatement() =>
        new MockMethodImplementationGeneratorSwitchBody<TParam1, TReturnType>();
}

public class MockMethodBuilderStage4WithSomeParam<TReturnType> : IMethodBuilderStage4WithSomeParam<TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<object, TReturnType> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory) => new MockMethodBodyGenerator();
}

public class MockMethodBuilderStage4WithSomeReturnTypeNoParam : IMethodBuilderStage4WithSomeReturnTypeNoParam
{
    public IMethodBodyGenerator UseProvidedBody(Func<object> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory) => new MockMethodBodyGenerator();
}

public class MockMethodBuilderStage4WithSomeReturnType<TParam1> : IMethodBuilderStage4WithSomeReturnType<TParam1>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, object> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory) => new MockMethodBodyGenerator();
}

public class MockMethodBuilderStage4WithSomeReturnTypeWithSomeParam : IMethodBuilderStage4WithSomeReturnTypeWithSomeParam
{
    public IMethodBodyGenerator UseProvidedBody(Func<object, object> body) => new MockMethodBodyGenerator();
    public IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory) => new MockMethodBodyGenerator();
}