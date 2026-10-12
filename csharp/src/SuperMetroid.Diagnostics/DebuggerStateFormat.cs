namespace SuperMetroid.Desktop;

/// <summary>Shared on-disk debugger-state header identities and compatibility versions.</summary>
internal static class DebuggerStateFormat
{
    /// <summary>Gets magic.</summary>
    public static ReadOnlySpan<byte> Magic => "SMCSTATE"u8;
    /// <summary>Stores slot count.</summary>
    public const int SlotCount = 10;
    /// <summary>Separate recovery slot; numbered manual slots remain zero through nine.</summary>
    public const int AutomaticSlot = SlotCount;
    /// <summary>Stores guid bytes.</summary>
    public const int GuidBytes = 16;
    /// <summary>Stores digest bytes.</summary>
    public const int DigestBytes = 32;
}

/// <summary>
/// The on-disk debugger-state schema, stored as a 32-bit header word. A build reads only its own
/// version and object layout (#1270): older states are rejected, never converted.
/// </summary>
internal enum DebuggerStateVersion
{
    /// <summary>
    /// Named delegate signatures, the installed-content identity with its named component table,
    /// and the build's exact object-graph layout.
    /// </summary>
    Current = 5,
}
