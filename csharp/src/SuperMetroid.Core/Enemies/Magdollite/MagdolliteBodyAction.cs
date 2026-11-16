namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a Magdollite's rising body, its arm, is doing. Its values are arbitrary.
/// </summary>
public enum MagdolliteBodyAction
{
    /// <summary>Holds the arm dormant until the head clears its not-emerged flag, then initializes growth at its spawn Y.</summary>
    Dormant,
    /// <summary>Rises at the head's speed and extends pillar segments until reaching Samus's height or the maximum rise.</summary>
    Rising,
    /// <summary>Holds the grown arm still at its apex while the hand throws.</summary>
    HoldingAtApex,
    /// <summary>Sinks at the head's speed, removes pillar segments, and releases the head when the arm lands.</summary>
    Falling,
}
