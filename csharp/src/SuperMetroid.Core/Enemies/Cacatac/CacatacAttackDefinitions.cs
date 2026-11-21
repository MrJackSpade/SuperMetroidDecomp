namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>The random roll that starts a Cacatac's spike attack.</summary>
internal static class CacatacAttackDefinitions
{
    /// <summary>
    /// The random field a patrolling Cacatac's attack roll starts from. The roll adds the enemy's update count, so the
    /// updates it attacks on move on as the count advances.
    /// </summary>
    internal static readonly RandomSampleField AttackRoll = RandomSampleField.OneOf(256);

    /// <summary>
    /// The chance a patrolling Cacatac stops to fire its spikes on an update (#1619): the lowest three of the 256 values its
    /// <see cref="AttackRoll"/> reaches once advanced by the enemy's frame counter, three updates in 256.
    /// </summary>
    internal static readonly RandomChance Attacks = RandomChance.Lowest(3, AttackRoll);
}
