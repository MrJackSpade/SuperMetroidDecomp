namespace SuperMetroid.Core;

/// <summary>
/// Marks an assembly whose types the compiler gives the members a debugger state reads, writes and restores them through
/// (#2602): every field a state holds is named in code the compiler writes, so a state reaches nothing by name at run time.
/// In a marked assembly every class, struct and record is declared <see langword="partial"/>, so the compiler can add those
/// members to it, unless <see cref="PartialTypesOnly"/> limits them to the types declared partial.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed partial class StateGraphAssemblyAttribute : Attribute
{
    /// <summary>
    /// Whether only the types declared <see langword="partial"/> get the members; otherwise every type must be declared so.
    /// </summary>
    public bool PartialTypesOnly { get; init; }

    /// <summary>
    /// Whether the objects the assembly's static read-only members hold are definition catalogs, which a state names by
    /// path rather than copying (#1623).
    /// </summary>
    public bool Catalogs { get; init; }
}
