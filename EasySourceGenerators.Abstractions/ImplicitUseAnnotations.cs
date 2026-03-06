namespace EasySourceGenerators.Abstractions;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
internal sealed class UsedImplicitlyAttribute : Attribute
{
    public UsedImplicitlyAttribute()
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Interface, Inherited = false)]
internal sealed class MeansImplicitUseAttribute : Attribute
{
    public MeansImplicitUseAttribute()
    {
    }

    public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags)
    {
    }

    public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags)
    {
    }

    public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
    {
    }
}

[Flags]
internal enum ImplicitUseKindFlags
{
    Default = Access | Assign | InstantiatedWithFixedConstructorSignature,
    Access = 1,
    Assign = 2,
    InstantiatedWithFixedConstructorSignature = 4,
    InstantiatedNoFixedConstructorSignature = 8
}

[Flags]
internal enum ImplicitUseTargetFlags
{
    Default = Itself,
    Itself = 1,
    Members = 2,
    WithMembers = Itself | Members
}
