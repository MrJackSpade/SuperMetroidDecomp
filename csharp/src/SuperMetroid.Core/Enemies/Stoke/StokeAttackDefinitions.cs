namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>The random roll that starts a Stoke's attack.</summary>
internal static class StokeAttackDefinitions
{
    /// <summary>
    /// The random field a moving Stoke's attack roll starts from: the sample modulo 256, 0 through 255. The roll adds the
    /// enemy's frame counter and continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField AttackRoll = new(0, 8);

    /// <summary>
    /// The chance a moving Stoke attacks on an update (#1619): the lowest two of the 256 values its
    /// <see cref="AttackRoll"/> reaches once advanced by the enemy's frame counter, two updates in 256.
    /// </summary>
    internal static readonly RandomChance Attacks = RandomChance.Lowest(2, AttackRoll);
}
