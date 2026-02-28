namespace MattSourceGenHelpers.Abstractions;

public class SwitchBodyRecord
{
    public List<object> CaseKeys { get; } = new();
    public List<object?> CaseValues { get; } = new();
    public bool HasDefaultCase { get; set; }
}

public class RecordingGeneratorsFactory : IGeneratorsFactory
{
    public SwitchBodyRecord? LastRecord { get; private set; }

    public IMethodImplementationGenerator CreateImplementation()
    {
        var record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGenerator(record);
    }

    public IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>()
    {
        var record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGeneratorTyped<TReturnType>(record);
    }

    public IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>()
    {
        var record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGenerator : IMethodImplementationGenerator
{
    private readonly SwitchBodyRecord _record;

    public RecordingMethodImplementationGenerator(SwitchBodyRecord record)
    {
        _record = record;
    }

    public IMethodImplementationGenerator WithBody(Action body) => this;
    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
}

public class RecordingMethodImplementationGeneratorTyped<TReturnType> : IMethodImplementationGenerator<TReturnType>
{
    private readonly SwitchBodyRecord _record;

    public RecordingMethodImplementationGeneratorTyped(SwitchBodyRecord record)
    {
        _record = record;
    }

    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
}

public class RecordingMethodImplementationGenerator<TArg1, TReturnType>
    : IMethodImplementationGenerator<TArg1, TReturnType>
{
    private readonly SwitchBodyRecord _record;

    public RecordingMethodImplementationGenerator(SwitchBodyRecord record)
    {
        _record = record;
    }

    public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> WithSwitchBody()
    {
        return new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(_record);
    }

    public IMethodImplementationGenerator WithBody(Action body) => this;
    public IMethodImplementationGenerator WithBody(Func<object> body) => this;
}

public class RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>
    : IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>
{
    private readonly SwitchBodyRecord _record;

    public RecordingMethodImplementationGeneratorSwitchBody(SwitchBodyRecord record)
    {
        _record = record;
    }

    public IMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType> ForCases(params object[] cases)
    {
        var flatCases = FlattenCases(cases).ToList();
        return new RecordingMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>(_record, flatCases);
    }

    public IMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
    {
        return new RecordingMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>(_record);
    }

    private static IEnumerable<TArg1> FlattenCases(object[] cases)
    {
        foreach (var c in cases)
        {
            if (c is TArg1[] arr)
            {
                foreach (var item in arr)
                    yield return item;
            }
            else if (c is TArg1 val)
            {
                yield return val;
            }
            else
            {
                yield return (TArg1)Convert.ChangeType(c, typeof(TArg1));
            }
        }
    }
}

public class RecordingMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>
    : IMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    private readonly SwitchBodyRecord _record;
    private readonly List<TArg1> _cases;

    public RecordingMethodImplementationGeneratorSwitchBodyCase(SwitchBodyRecord record, List<TArg1> cases)
    {
        _record = record;
        _cases = cases;
    }

    public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> constantValueFactory)
    {
        foreach (var caseValue in _cases)
        {
            var result = constantValueFactory(caseValue);
            _record.CaseKeys.Add((object)caseValue ?? throw new InvalidOperationException("Switch case value cannot be null"));
            _record.CaseValues.Add(result);
        }
        return new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(_record);
    }

    public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> UseBody(Func<TArg1, Action<TReturnType>> body)
    {
        foreach (var caseValue in _cases)
        {
            _record.CaseKeys.Add((object)caseValue ?? throw new InvalidOperationException("Switch case value cannot be null"));
            _record.CaseValues.Add(null);
        }
        return new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(_record);
    }
}

public class RecordingMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>
    : IMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>
{
    private readonly SwitchBodyRecord _record;

    public RecordingMethodImplementationGeneratorSwitchBodyDefaultCase(SwitchBodyRecord record)
    {
        _record = record;
    }

    public IMethodImplementationGenerator<TArg1, TReturnType> CompileTimeBody(Func<TArg1, TReturnType> func)
    {
        _record.HasDefaultCase = true;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(_record);
    }

    public IMethodImplementationGenerator<TArg1, TReturnType> WithBody(Func<TArg1, Func<TReturnType>> func)
    {
        _record.HasDefaultCase = true;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(_record);
    }
}
