namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The callbacks of Kraid's six-byte sinking/death records; each member names the bank-$A7 routine it ports.
/// </summary>
public enum KraidSinkCallback
{
    /// <summary>
    /// <c>RTS_A7C6A6</c> at $A7:C6A6. Rows that only clear another BG2 tilemap strip
    /// select this callback so the indirect JSR returns without spawning rocks or PLMs.
    /// </summary>
    NoOperation,

    /// <summary><c>CrumbleLeftPlatform_Left</c> at $A7:C691.</summary>
    CrumbleLeftPlatformLeft,

    /// <summary><c>CrumbleRightPlatform_Middle</c> at $A7:C6A7.</summary>
    CrumbleRightPlatformMiddle,

    /// <summary><c>CrumbleRightPlatform_Left</c> at $A7:C6BD.</summary>
    CrumbleRightPlatformLeft,

    /// <summary><c>CrumbleLeftPlatform_Right</c> at $A7:C6D3.</summary>
    CrumbleLeftPlatformRight,

    /// <summary><c>CrumbleLeftPlatform_Middle</c> at $A7:C6E9.</summary>
    CrumbleLeftPlatformMiddle,

    /// <summary><c>CrumbleRightPlatform_Right</c> at $A7:C6FF.</summary>
    CrumbleRightPlatformRight,
}
