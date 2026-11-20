namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops (#641).
/// Their steps are <see cref="BoyonAnimations"/>.
/// </summary>
internal enum BoyonSegment
{
    /// <summary>The idle's start, its entry cue.</summary>
    Idle,
    /// <summary>The idle's resting poses.</summary>
    IdleRest,
    /// <summary>The bounce's start, its entry cues.</summary>
    Bouncing,
    /// <summary>The bounce's star poses.</summary>
    BouncingStar,
}
