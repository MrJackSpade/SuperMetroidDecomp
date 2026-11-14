namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The GRipper, Ripper II and Ripper flying loops (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="RipperAnimations"/>.
/// </summary>
internal enum RipperSegment
{
    /// <summary>The g Ripper moving left animation.</summary>
    GRipperMovingLeft,
    /// <summary>The g Ripper moving right animation.</summary>
    GRipperMovingRight,
    /// <summary>The Ripper moving right animation.</summary>
    RipperMovingRight,
    /// <summary>The Ripper moving left animation.</summary>
    RipperMovingLeft,
}
