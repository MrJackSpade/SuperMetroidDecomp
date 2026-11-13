namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>The random roll that starts a Cacatac's spike attack.</summary>
internal static class CacatacAttackDefinitions
{
    /// <summary>
    /// The random field a patrolling Cacatac's attack roll starts from: the sample modulo 256, 0 through 255. The roll adds
    /// the enemy's frame counter and continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField AttackRoll = new(0, 8);

    /// <summary>
    /// The chance a patrolling Cacatac stops to fire its spikes on an update (#1619): the lowest three of the 256 values its
    /// <see cref="AttackRoll"/> reaches once advanced by the enemy's frame counter, three updates in 256.
    /// </summary>
    internal static readonly RandomChance Attacks = RandomChance.Lowest(3, AttackRoll);
}
