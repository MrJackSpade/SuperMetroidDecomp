namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="AtomicAnimations"/>.
/// </summary>
internal enum AtomicSegment
{
    /// <summary>Turning one way through its six up-right poses, a sixth of a turn each, repeating.</summary>
    UpRightTurning,
    /// <summary>Turning one way through its six up-left poses, a sixth of a turn each, repeating.</summary>
    UpLeftTurning,
    /// <summary>Turning back the other way through its six up-right poses, in reverse, repeating (#2369).</summary>
    UpRightTurningBack,
    /// <summary>Turning back the other way through its six up-left poses, in reverse, repeating (#2369).</summary>
    UpLeftTurningBack,
}
