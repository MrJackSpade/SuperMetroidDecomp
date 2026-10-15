namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Shared on-disk debugger-state header identities and compatibility versions.</summary>
internal static class DebuggerStateFormat
{
    /// <summary>Eight-byte signature at the start of every debugger-state file, checked before decoding its payload.</summary>
    public static ReadOnlySpan<byte> Magic => "SMCSTATE"u8;
    /// <summary>Number of numbered manual save slots, excluding the separate automatic recovery slot.</summary>
    public const int SlotCount = 10;
    /// <summary>Separate recovery slot; numbered manual slots remain zero through nine.</summary>
    public const int AutomaticSlot = SlotCount;
    /// <summary>Byte width of the content identity GUID stored in the state header.</summary>
    public const int GuidBytes = 16;
    /// <summary>Byte width of the digest that ties a state file to its installed content.</summary>
    public const int DigestBytes = 32;
}
