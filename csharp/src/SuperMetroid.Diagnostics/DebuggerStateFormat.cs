namespace SuperMetroid.Desktop;

/// <summary>Shared on-disk debugger-state header identities and compatibility versions.</summary>
internal static class DebuggerStateFormat
{
    /// <summary>Eight-byte file signature identifying a debugger-state envelope.</summary>
    public static ReadOnlySpan<byte> Magic => "SMCSTATE"u8;
    /// <summary>Version two stores CLR metadata tokens for delegate methods.</summary>
    public const int LegacyTokenVersion = 2;
    /// <summary>Version three stores named delegate signatures without changing object fields.</summary>
    public const int NamedDelegateVersion = 3;
    /// <summary>Version four adds selected installed-content identity to the state header.</summary>
    public const int IdentifiedVersion = 4;
    /// <summary>Version five adds the bounded named-presentation-catalog identity table.</summary>
    public const int CurrentVersion = 5;
    /// <summary>Number of manual save-state slots addressable by the UI.</summary>
    public const int SlotCount = 10;
    /// <summary>Separate recovery slot; numbered manual slots remain zero through nine.</summary>
    public const int AutomaticSlot = SlotCount;
    /// <summary>Byte length of the content-identity GUID stored in the header.</summary>
    public const int GuidBytes = 16;
    /// <summary>Byte length of the SHA-256 digest stored in the header.</summary>
    public const int DigestBytes = 32;
}
