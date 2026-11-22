namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>One enum's table of the members a packed run holds.</summary>
internal interface IPackedEnumTable
{
    /// <summary>The enum.</summary>
    Type Enum { get; }

    /// <summary>Writes the enum's name and its members' names.</summary>
    void Write(GraphWriter writer);

    /// <summary>The member at <paramref name="index"/> of the table, boxed.</summary>
    /// <exception cref="InvalidDataException">The run holds no such member.</exception>
    object MemberAt(int index);
}
