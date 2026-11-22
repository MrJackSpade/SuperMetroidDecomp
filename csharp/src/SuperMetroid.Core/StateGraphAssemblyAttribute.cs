namespace SuperMetroid.Core;

/// <summary>
/// Marks a project whose types get generated members a game snapshot is written, read and restored through (#2602): every
/// field a snapshot holds is named in generated code, so a snapshot reaches nothing by name while the game runs. Every
/// class, struct and record of a marked project is declared <see langword="partial"/>, so the generated members can join
/// it, unless <see cref="PartialTypesOnly"/> limits them to the types declared so.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed partial class StateGraphAssemblyAttribute : Attribute
{
    /// <summary>Whether only the types declared <see langword="partial"/> get the members.</summary>
    public bool PartialTypesOnly { get; init; }

    /// <summary>
    /// Whether the objects the project's static read-only members hold are definition catalogs, which a snapshot names by
    /// path rather than copying (#1623).
    /// </summary>
    public bool Catalogs { get; init; }
}
