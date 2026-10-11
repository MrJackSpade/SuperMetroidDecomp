namespace SuperMetroid.Desktop;

/// <summary>Shared on-disk debugger-state header identities and compatibility versions.</summary>
internal static class DebuggerStateFormat
{
    public static ReadOnlySpan<byte> Magic => "SMCSTATE"u8;
    public const int SlotCount = 10;
    /// <summary>Separate recovery slot; numbered manual slots remain zero through nine.</summary>
    public const int AutomaticSlot = SlotCount;
    public const int GuidBytes = 16;
    public const int DigestBytes = 32;
}

/// <summary>The on-disk debugger-state schema versions this build reads, stored as a 32-bit header word.</summary>
internal enum DebuggerStateVersion
{
    /// <summary>Version two stores CLR metadata tokens for delegate methods.</summary>
    LegacyToken = 2,
    /// <summary>Version three stores named delegate signatures without changing object fields.</summary>
    NamedDelegate = 3,
    /// <summary>Version four adds selected installed-content identity to the state header.</summary>
    Identified = 4,
    /// <summary>Version five adds the bounded named-presentation-catalog identity table.</summary>
    Current = 5,
}
