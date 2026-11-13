namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's stationary corpse pose as a typed animation (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="DeadTorizoAnimations"/>.
/// </summary>
internal enum DeadTorizoSegment
{
    /// <summary>The stationary animation.</summary>
    Stationary,
}
