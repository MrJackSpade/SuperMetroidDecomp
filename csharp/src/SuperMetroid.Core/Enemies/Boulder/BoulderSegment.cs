namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops (#641).
/// Their steps are <see cref="BoulderAnimations"/>.
/// </summary>
internal enum BoulderSegment
{
    /// <summary>Rolling left, turning an eighth of a turn each pose, repeating.</summary>
    Left,
    /// <summary>Rolling right, turning an eighth of a turn the other way each pose, repeating.</summary>
    Right,
}
