namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The platform crumbles of Kraid's sinking schedule (#1889); each member names the block its step crumbles.
/// </summary>
public enum KraidSinkStep
{
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
