namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The grapple Ripper, jet Ripper and Ripper flying loops (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="RipperAnimations"/>.
/// </summary>
internal enum RipperSegment
{
    /// <summary>The grapple Ripper moving left animation.</summary>
    GrappleRipperMovingLeft,
    /// <summary>The grapple Ripper moving right animation.</summary>
    GrappleRipperMovingRight,
    /// <summary>The Ripper moving right animation.</summary>
    RipperMovingRight,
    /// <summary>The Ripper moving left animation.</summary>
    RipperMovingLeft,
}
