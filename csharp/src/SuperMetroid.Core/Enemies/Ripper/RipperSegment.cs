namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The grapple Ripper, jet Ripper and Ripper flying loops (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="RipperAnimations"/>.
/// </summary>
internal enum RipperSegment
{
    /// <summary>A grapple Ripper flying left, its jet flame flickering short, medium and long, repeating.</summary>
    GrappleRipperMovingLeft,
    /// <summary>A grapple Ripper flying right, its jet flame flickering short, medium and long, repeating.</summary>
    GrappleRipperMovingRight,
    /// <summary>A Ripper flying right, its eye lit, dark and glinting, repeating.</summary>
    RipperMovingRight,
    /// <summary>A Ripper flying left, its eye lit, dark and glinting, repeating.</summary>
    RipperMovingLeft,
}
