namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>What a value in a debugger state begins with (#2602).</summary>
internal enum ObjectMarker : byte
{
    /// <summary>No value.</summary>
    Null,

    /// <summary>An object defined earlier, by its number.</summary>
    Reference,

    /// <summary>A new value: its number (zero for a value type), its type's name and its payload.</summary>
    New,

    /// <summary>An object the host bound into the session, by its number and path (#1507).</summary>
    Bound,

    /// <summary>An object the game's static members hold, by its number and path (#1623).</summary>
    Definition,
}
