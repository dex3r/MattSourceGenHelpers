using System;
using System.Collections.Generic;
using System.Linq;
using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Abstractions.Method;

namespace EasySourceGenerators.Generators;

public class SwitchBodyRecord
{
    public List<object> CaseKeys { get; } = new();
    public List<object?> CaseValues { get; } = new();
    public bool HasDefaultCase { get; set; }
}

public class MethodRecord
{
    public string? MethodName { get; set; }
    public string? ReturnTypeName { get; set; }
    public List<string> ParameterTypeNames { get; } = new();
    public object? ConstantValue { get; set; }
    public SwitchBodyRecord? SwitchBody { get; set; }
    public bool IsEntireMethodGeneration { get; set; }
}

public class RecordingGeneratorsFactory : IMethodBodyGeneratorStage0, IGeneratorsFactory
{
    public SwitchBodyRecord? LastRecord { get; private set; }
    public MethodRecord? LastMethodRecord { get; private set; }

    public IMethodBodyBuilderStage1 StartFluentApiBuilderForBody() => new MethodBodyBuilder(this);

    public IMethodBuilderStage1 StartFluentApiBuilderForMethod()
    {
        MethodRecord record = new MethodRecord();
        record.IsEntireMethodGeneration = true;
        LastMethodRecord = record;
        return new RecordingMethodBuilderStage1(record, this);
    }

    public IMethodBodyGeneratorWithNoParameter CreateImplementation()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGenerator();
    }

    public IMethodBodyBuilder ForMethod() => new MethodBodyBuilder(this);

    public IMethodBodyGenerator<TReturnType> CreateImplementation<TReturnType>()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGeneratorTyped<TReturnType>();
    }

    public IMethodBodyGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(record);
    }

    internal void SetSwitchBodyFromMethod(SwitchBodyRecord record)
    {
        LastRecord = record;
    }
}

public class RecordingMethodImplementationGenerator : IMethodBodyGeneratorWithNoParameter;

public class RecordingMethodImplementationGeneratorTyped<TReturnType> : IMethodBodyGenerator<TReturnType>
{
    public IMethodBodyGeneratorWithNoParameter BodyReturningConstantValue(Func<object> body) =>
        new RecordingMethodImplementationGenerator();
}

public class RecordingMethodImplementationGenerator<TArg1, TReturnType>(SwitchBodyRecord record) : IMethodBodyGenerator<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> GenerateSwitchBody()
    {
        return new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(SwitchBodyRecord record)
    : IMethodBodyGeneratorSwitchBody<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType> ForCases(params TArg1[] cases)
    {
        List<TArg1> flatCases = cases.ToList();
        return new RecordingMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>(record, flatCases);
    }

    public IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
    {
        return new RecordingMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>(SwitchBodyRecord record, List<TArg1> cases)
    : IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyCaseStage2<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> constantValueFactory)
    {
        foreach (TArg1? caseValue in cases)
        {
            TReturnType result = constantValueFactory(caseValue);
            record.CaseKeys.Add((object?)caseValue ?? throw new InvalidOperationException("Switch case value cannot be null"));
            record.CaseValues.Add(result);
        }
        return new RecordingMethodImplementationGeneratorSwitchBodyCaseStage2<TArg1, TReturnType>(record);
    }

    public IMethodBodyGeneratorSwitchBodyCaseStage2<TArg1, TReturnType> UseProvidedBody(Func<TArg1, TReturnType> body)
    {
        foreach (TArg1? caseValue in cases)
        {
            record.CaseKeys.Add((object?)caseValue ?? throw new InvalidOperationException("Switch case value cannot be null"));
            record.CaseValues.Add(null);
        }
        return new RecordingMethodImplementationGeneratorSwitchBodyCaseStage2<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGeneratorSwitchBodyCaseStage2<TArg1, TReturnType>(SwitchBodyRecord record)
    : IMethodBodyGeneratorSwitchBodyCaseStage2<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
    {
        return new RecordingMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>(SwitchBodyRecord record)
    : IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>
{
    public IMethodBodyGenerator ReturnConstantValue(Func<TArg1, TReturnType> func)
    {
        record.HasDefaultCase = true;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(record);
    }

    public IMethodBodyGenerator UseProvidedBody(Func<TArg1, TReturnType> func)
    {
        record.HasDefaultCase = true;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(record);
    }
}

// Recording implementations for Method builder (entire method generation)
public class RecordingMethodBuilderStage1(MethodRecord methodRecord, RecordingGeneratorsFactory factory) : IMethodBuilderStage1
{
    public IMethodBuilderStage2 WithName(string name)
    {
        methodRecord.MethodName = name;
        return new RecordingMethodBuilderStage2(methodRecord, factory);
    }

    public IMethodBuilderStage2 WithName(Func<string> nameFactory)
    {
        methodRecord.MethodName = nameFactory();
        return new RecordingMethodBuilderStage2(methodRecord, factory);
    }
}

public class RecordingMethodBuilderStage2(MethodRecord methodRecord, RecordingGeneratorsFactory factory) : IMethodBuilderStage2
{
    public IMethodBuilderStage3ReturningVoid WithVoidReturn()
    {
        methodRecord.ReturnTypeName = "void";
        return new RecordingMethodBuilderStage3ReturningVoid(methodRecord, factory);
    }

    public IMethodBuilderStage3<TReturnType> WithReturnType<TReturnType>()
    {
        methodRecord.ReturnTypeName = typeof(TReturnType).FullName ?? typeof(TReturnType).Name;
        return new RecordingMethodBuilderStage3<TReturnType>(methodRecord, factory);
    }

    public IMethodBuilderStage3WithSomeReturnType WithReturnType(Type returnType)
    {
        methodRecord.ReturnTypeName = returnType.FullName ?? returnType.Name;
        return new RecordingMethodBuilderStage3WithSomeReturnType(methodRecord, factory);
    }

    public IMethodBuilderStage3WithSomeReturnType WithReturnType(string returnType)
    {
        methodRecord.ReturnTypeName = returnType;
        return new RecordingMethodBuilderStage3WithSomeReturnType(methodRecord, factory);
    }

    public IMethodBuilderStage3WithSomeReturnType WithReturnType(Func<Type> returnTypeFactory)
    {
        Type returnType = returnTypeFactory();
        methodRecord.ReturnTypeName = returnType.FullName ?? returnType.Name;
        return new RecordingMethodBuilderStage3WithSomeReturnType(methodRecord, factory);
    }

    public IMethodBuilderStage3WithSomeReturnType WithReturnType(Func<string> returnTypeFactory)
    {
        methodRecord.ReturnTypeName = returnTypeFactory();
        return new RecordingMethodBuilderStage3WithSomeReturnType(methodRecord, factory);
    }
}

public class RecordingMethodBuilderStage3ReturningVoid(MethodRecord methodRecord, RecordingGeneratorsFactory factory) : IMethodBuilderStage3ReturningVoid
{
    public IMethodBuilderStage4ReturnVoidNoParam WithNoParameters() =>
        new RecordingMethodBuilderStage4ReturnVoidNoParam(methodRecord);

    public IMethodBuilderStage4ReturnVoid<TParam1> WithParameter<TParam1>()
    {
        methodRecord.ParameterTypeNames.Add(typeof(TParam1).FullName ?? typeof(TParam1).Name);
        return new RecordingMethodBuilderStage4ReturnVoid<TParam1>(methodRecord);
    }

    public IMethodBuilderStage4ReturnVoid WithParameter(Type param1)
    {
        methodRecord.ParameterTypeNames.Add(param1.FullName ?? param1.Name);
        return new RecordingMethodBuilderStage4ReturnVoidNonGeneric(methodRecord);
    }

    public IMethodBuilderStage4ReturnVoid WithParameter(string param1)
    {
        methodRecord.ParameterTypeNames.Add(param1);
        return new RecordingMethodBuilderStage4ReturnVoidNonGeneric(methodRecord);
    }

    public IMethodBuilderStage4ReturnVoid WithParameter(Func<Type> param1Factory)
    {
        Type param1 = param1Factory();
        methodRecord.ParameterTypeNames.Add(param1.FullName ?? param1.Name);
        return new RecordingMethodBuilderStage4ReturnVoidNonGeneric(methodRecord);
    }

    public IMethodBuilderStage4ReturnVoid WithParameter(Func<string> param1Factory)
    {
        methodRecord.ParameterTypeNames.Add(param1Factory());
        return new RecordingMethodBuilderStage4ReturnVoidNonGeneric(methodRecord);
    }
}

public class RecordingMethodBuilderStage3<TReturnType>(MethodRecord methodRecord, RecordingGeneratorsFactory factory) : IMethodBuilderStage3<TReturnType>
{
    public IMethodBuilderStage4NoParam<TReturnType> WithNoParameters() =>
        new RecordingMethodBuilderStage4NoParam<TReturnType>(methodRecord, factory);

    public IMethodBuilderStage4<TParam1, TReturnType> WithParameter<TParam1>()
    {
        methodRecord.ParameterTypeNames.Add(typeof(TParam1).FullName ?? typeof(TParam1).Name);
        return new RecordingMethodBuilderStage4<TParam1, TReturnType>(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Type param1)
    {
        methodRecord.ParameterTypeNames.Add(param1.FullName ?? param1.Name);
        return new RecordingMethodBuilderStage4WithSomeParam<TReturnType>(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(string param1)
    {
        methodRecord.ParameterTypeNames.Add(param1);
        return new RecordingMethodBuilderStage4WithSomeParam<TReturnType>(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Func<Type> param1Factory)
    {
        Type param1 = param1Factory();
        methodRecord.ParameterTypeNames.Add(param1.FullName ?? param1.Name);
        return new RecordingMethodBuilderStage4WithSomeParam<TReturnType>(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeParam<TReturnType> WithParameter(Func<string> param1Factory)
    {
        methodRecord.ParameterTypeNames.Add(param1Factory());
        return new RecordingMethodBuilderStage4WithSomeParam<TReturnType>(methodRecord, factory);
    }
}

public class RecordingMethodBuilderStage3WithSomeReturnType(MethodRecord methodRecord, RecordingGeneratorsFactory factory) : IMethodBuilderStage3WithSomeReturnType
{
    public IMethodBuilderStage4WithSomeReturnTypeNoParam WithNoParameters() =>
        new RecordingMethodBuilderStage4WithSomeReturnTypeNoParam(methodRecord, factory);

    public IMethodBuilderStage4WithSomeReturnType<TParam1> WithParameter<TParam1>()
    {
        methodRecord.ParameterTypeNames.Add(typeof(TParam1).FullName ?? typeof(TParam1).Name);
        return new RecordingMethodBuilderStage4WithSomeReturnType<TParam1>(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Type param1)
    {
        methodRecord.ParameterTypeNames.Add(param1.FullName ?? param1.Name);
        return new RecordingMethodBuilderStage4WithSomeReturnTypeWithSomeParam(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(string param1)
    {
        methodRecord.ParameterTypeNames.Add(param1);
        return new RecordingMethodBuilderStage4WithSomeReturnTypeWithSomeParam(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Func<Type> param1Factory)
    {
        Type param1 = param1Factory();
        methodRecord.ParameterTypeNames.Add(param1.FullName ?? param1.Name);
        return new RecordingMethodBuilderStage4WithSomeReturnTypeWithSomeParam(methodRecord, factory);
    }

    public IMethodBuilderStage4WithSomeReturnTypeWithSomeParam WithParameter(Func<string> param1Factory)
    {
        methodRecord.ParameterTypeNames.Add(param1Factory());
        return new RecordingMethodBuilderStage4WithSomeReturnTypeWithSomeParam(methodRecord, factory);
    }
}

// Stage4 recording implementations - Terminal methods
public class RecordingMethodBuilderStage4ReturnVoidNoParam(MethodRecord methodRecord) : IMethodBuilderStage4ReturnVoidNoParam
{
    public IMethodBodyGenerator UseProvidedBody(Action body) => new RecordingMethodImplementationGenerator();
}

public class RecordingMethodBuilderStage4ReturnVoid<TParam1>(MethodRecord methodRecord) : IMethodBuilderStage4ReturnVoid<TParam1>
{
    public IMethodBodyGenerator UseProvidedBody(Action<TParam1> body) => new RecordingMethodImplementationGenerator();
}

public class RecordingMethodBuilderStage4ReturnVoidNonGeneric(MethodRecord methodRecord) : IMethodBuilderStage4ReturnVoid
{
    public IMethodBodyGenerator UseProvidedBody(Action<object> body) => new RecordingMethodImplementationGenerator();
}

public class RecordingMethodBuilderStage4NoParam<TReturnType>(MethodRecord methodRecord, RecordingGeneratorsFactory factory)
    : IMethodBuilderStage4NoParam<TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body) => new RecordingMethodImplementationGenerator();

    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory)
    {
        object? value = constantValueFactory();
        methodRecord.ConstantValue = value;
        return new RecordingMethodImplementationGenerator();
    }
}

public class RecordingMethodBuilderStage4<TParam1, TReturnType>(MethodRecord methodRecord, RecordingGeneratorsFactory factory)
    : IMethodBuilderStage4<TParam1, TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body) => new RecordingMethodImplementationGenerator();

    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory)
    {
        object? value = constantValueFactory();
        methodRecord.ConstantValue = value;
        return new RecordingMethodImplementationGenerator();
    }

    public IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> BodyWithSwitchStatement()
    {
        SwitchBodyRecord switchRecord = new SwitchBodyRecord();
        methodRecord.SwitchBody = switchRecord;
        factory.SetSwitchBodyFromMethod(switchRecord);
        return new RecordingMethodImplementationGeneratorSwitchBody<TParam1, TReturnType>(switchRecord);
    }
}

public class RecordingMethodBuilderStage4WithSomeParam<TReturnType>(MethodRecord methodRecord, RecordingGeneratorsFactory factory)
    : IMethodBuilderStage4WithSomeParam<TReturnType>
{
    public IMethodBodyGenerator UseProvidedBody(Func<object, TReturnType> body) => new RecordingMethodImplementationGenerator();

    public IMethodBodyGenerator BodyReturningConstant(Func<TReturnType> constantValueFactory)
    {
        object? value = constantValueFactory();
        methodRecord.ConstantValue = value;
        return new RecordingMethodImplementationGenerator();
    }
}

public class RecordingMethodBuilderStage4WithSomeReturnTypeNoParam(MethodRecord methodRecord, RecordingGeneratorsFactory factory)
    : IMethodBuilderStage4WithSomeReturnTypeNoParam
{
    public IMethodBodyGenerator UseProvidedBody(Func<object> body) => new RecordingMethodImplementationGenerator();

    public IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory)
    {
        object? value = constantValueFactory();
        methodRecord.ConstantValue = value;
        return new RecordingMethodImplementationGenerator();
    }
}

public class RecordingMethodBuilderStage4WithSomeReturnType<TParam1>(MethodRecord methodRecord, RecordingGeneratorsFactory factory)
    : IMethodBuilderStage4WithSomeReturnType<TParam1>
{
    public IMethodBodyGenerator UseProvidedBody(Func<TParam1, object> body) => new RecordingMethodImplementationGenerator();

    public IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory)
    {
        object? value = constantValueFactory();
        methodRecord.ConstantValue = value;
        return new RecordingMethodImplementationGenerator();
    }
}

public class RecordingMethodBuilderStage4WithSomeReturnTypeWithSomeParam(MethodRecord methodRecord, RecordingGeneratorsFactory factory)
    : IMethodBuilderStage4WithSomeReturnTypeWithSomeParam
{
    public IMethodBodyGenerator UseProvidedBody(Func<object, object> body) => new RecordingMethodImplementationGenerator();

    public IMethodBodyGenerator BodyReturningConstant(Func<object> constantValueFactory)
    {
        object? value = constantValueFactory();
        methodRecord.ConstantValue = value;
        return new RecordingMethodImplementationGenerator();
    }
}
