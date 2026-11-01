namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The callbacks of Kraid's six-byte sinking/death records; each member names the routine it ports.
/// </summary>
public enum KraidSinkCallback
{
    /// <summary>
    /// The empty callback. Rows that only clear another strip of his body
    /// select this callback so it returns without spawning rocks or PLMs.
    /// </summary>
    NoOperation,

    /// <summary><c>CrumbleLeftPlatform_Left</c>.</summary>
    CrumbleLeftPlatformLeft,

    /// <summary><c>CrumbleRightPlatform_Middle</c>.</summary>
    CrumbleRightPlatformMiddle,

    /// <summary><c>CrumbleRightPlatform_Left</c>.</summary>
    CrumbleRightPlatformLeft,

    /// <summary><c>CrumbleLeftPlatform_Right</c>.</summary>
    CrumbleLeftPlatformRight,

    /// <summary><c>CrumbleLeftPlatform_Middle</c>.</summary>
    CrumbleLeftPlatformMiddle,

    /// <summary><c>CrumbleRightPlatform_Right</c>.</summary>
    CrumbleRightPlatformRight,
}
