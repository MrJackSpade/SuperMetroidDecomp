namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>One enum's table of the members a packed run holds.</summary>
internal interface IPackedEnumTable
{
    /// <summary>The enum.</summary>
    Type Enum { get; }

    /// <summary>Writes the enum's name and its members' names.</summary>
    void Write(BinaryWriter writer);
}
