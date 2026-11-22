namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>An enum whose members a packed run can hold.</summary>
internal interface IPackedEnumType
{
    /// <summary>Reads a run's table of the enum's members, after its name.</summary>
    IPackedEnumTable ReadTable(GraphReader reader);
}
