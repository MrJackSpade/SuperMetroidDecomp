namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a Magdollite's rising body, its arm, is doing. Its values are arbitrary.
/// </summary>
/// <remarks>
/// Released is a state of its course, not a request another owner stores for it (#2562): a cue, a touch or a shot ends
/// the state before, and the game acts on it one update later, the state naming where it stands until then; the update
/// that acts on it moves it on, as every other state's does. The update between is the game's.
/// </remarks>
public enum MagdolliteBodyAction
{
    /// <summary>Holds the arm dormant below its origin until its head releases it.</summary>
    Dormant,
    /// <summary>Released by its head (#2439): the next update returns the arm to its origin and starts it rising.</summary>
    Released,
    /// <summary>Rises at the head's speed and extends pillar segments until reaching Samus's height or the maximum rise.</summary>
    Rising,
    /// <summary>Holds the grown arm still at its apex while the hand throws.</summary>
    HoldingAtApex,
    /// <summary>Sinks at the head's speed, removes pillar segments, and releases the head when the arm lands.</summary>
    Falling,
}
