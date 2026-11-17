namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="AtomicAnimations"/>.
/// </summary>
internal enum AtomicSegment
{
    /// <summary>Turning one way through its six up-right poses, a sixth of a turn each, repeating.</summary>
    UpRight,
    /// <summary>Turning one way through its six up-left poses, a sixth of a turn each, repeating.</summary>
    UpLeft,
    /// <summary>Turning the other way through its six up-right poses, in reverse, repeating.</summary>
    DownLeft,
    /// <summary>Turning the other way through its six up-left poses, in reverse, repeating.</summary>
    DownRight,
}
