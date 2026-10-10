namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A7 callback pointers stored in Kraid's six-byte sinking/death records.
/// </summary>
public enum KraidSinkCallback : ushort
{
    /// <summary>
    /// <c>RTS_A7C6A6</c> at $A7:C6A6. Rows that only clear another BG2 tilemap strip
    /// select this callback so the indirect JSR returns without spawning rocks or PLMs.
    /// </summary>
    NoOperation = 0xc6a6,

    /// <summary><c>CrumbleLeftPlatform_Left</c> at $A7:C691.</summary>
    CrumbleLeftPlatformLeft = 0xc691,

    /// <summary><c>CrumbleRightPlatform_Middle</c> at $A7:C6A7.</summary>
    CrumbleRightPlatformMiddle = 0xc6a7,

    /// <summary><c>CrumbleRightPlatform_Left</c> at $A7:C6BD.</summary>
    CrumbleRightPlatformLeft = 0xc6bd,

    /// <summary><c>CrumbleLeftPlatform_Right</c> at $A7:C6D3.</summary>
    CrumbleLeftPlatformRight = 0xc6d3,

    /// <summary><c>CrumbleLeftPlatform_Middle</c> at $A7:C6E9.</summary>
    CrumbleLeftPlatformMiddle = 0xc6e9,

    /// <summary><c>CrumbleRightPlatform_Right</c> at $A7:C6FF.</summary>
    CrumbleRightPlatformRight = 0xc6ff,
}
