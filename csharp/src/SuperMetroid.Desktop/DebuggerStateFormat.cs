namespace SuperMetroid.Desktop;

/// <summary>On-disk debugger-state header identities and compatibility versions.</summary>
internal static class DebuggerStateFormat
{
    public static ReadOnlySpan<byte> Magic => "SMCSTATE"u8;
    /// <summary>Version two stores CLR metadata tokens for delegate methods.</summary>
    public const int LegacyTokenVersion = 2;
    /// <summary>Version three stores named delegate signatures without changing object fields.</summary>
    public const int CurrentVersion = 3;
    public const int SlotCount = 10;
    public const int GuidBytes = 16;
}
