namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The steps of Kraid's sinking/death schedule; each member names what its step does.
/// </summary>
public enum KraidSinkStep
{
    /// <summary>
    /// The empty step. Rows that only clear another strip of his body
    /// select this step so it returns without spawning rocks or room objects.
    /// </summary>
    NoOperation,

    /// <summary>Crumbles the left platform's left block.</summary>
    CrumbleLeftPlatformLeft,

    /// <summary>Crumbles the right platform's middle block.</summary>
    CrumbleRightPlatformMiddle,

    /// <summary>Crumbles the right platform's left block.</summary>
    CrumbleRightPlatformLeft,

    /// <summary>Crumbles the left platform's right block.</summary>
    CrumbleLeftPlatformRight,

    /// <summary>Crumbles the left platform's middle block.</summary>
    CrumbleLeftPlatformMiddle,

    /// <summary>Crumbles the right platform's right block.</summary>
    CrumbleRightPlatformRight,
}
