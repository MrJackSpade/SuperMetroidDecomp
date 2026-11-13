namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Choot's idle, jumping and falling animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="ChootAnimations"/>.
/// </summary>
internal enum ChootSegment
{
    /// <summary>The idle animation.</summary>
    Idle,
    /// <summary>The jumping animation.</summary>
    Jumping,
    /// <summary>The falling animation.</summary>
    Falling,
}
