namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>The random roll that starts a Cacatac's spike attack.</summary>
internal static class CacatacAttackDefinitions
{
    /// <summary>
    /// The random field a patrolling Cacatac's attack roll starts from: bits zero to seven, 0 through 255. The roll adds
    /// the enemy's frame counter and continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField AttackRoll = new(0, 8);
}
