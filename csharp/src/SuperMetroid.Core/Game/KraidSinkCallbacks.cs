namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A7 callback pointers stored in Kraid's six-byte sinking/death records.
/// </summary>
internal static class KraidSinkCallbacks
{
    /// <summary>
    /// <c>RTS_A7C6A6</c> at $A7:C6A6. Rows that only clear another BG2 tilemap strip
    /// select this callback so the indirect JSR returns without spawning rocks or PLMs.
    /// </summary>
    public const ushort NoOperation = 0xc6a6;

    /// <summary><c>CrumbleLeftPlatform_Left</c> at $A7:C691.</summary>
    public const ushort CrumbleLeftPlatformLeft = 0xc691;

    /// <summary><c>CrumbleRightPlatform_Middle</c> at $A7:C6A7.</summary>
    public const ushort CrumbleRightPlatformMiddle = 0xc6a7;

    /// <summary><c>CrumbleRightPlatform_Left</c> at $A7:C6BD.</summary>
    public const ushort CrumbleRightPlatformLeft = 0xc6bd;

    /// <summary><c>CrumbleLeftPlatform_Right</c> at $A7:C6D3.</summary>
    public const ushort CrumbleLeftPlatformRight = 0xc6d3;

    /// <summary><c>CrumbleLeftPlatform_Middle</c> at $A7:C6E9.</summary>
    public const ushort CrumbleLeftPlatformMiddle = 0xc6e9;

    /// <summary><c>CrumbleRightPlatform_Right</c> at $A7:C6FF.</summary>
    public const ushort CrumbleRightPlatformRight = 0xc6ff;
}
