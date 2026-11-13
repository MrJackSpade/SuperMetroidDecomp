namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="BoyonAnimations"/>.
/// </summary>
internal enum BoyonSegment
{
    /// <summary>The idle animation.</summary>
    Idle,
    /// <summary>The loop.</summary>
    IdleLoop,
    /// <summary>The bouncing animation.</summary>
    Bouncing,
    /// <summary>The loop.</summary>
    BouncingLoop,
}
