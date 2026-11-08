namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>The random roll that starts a Stoke's attack.</summary>
internal static class StokeAttackDefinitions
{
    /// <summary>
    /// The random field a moving Stoke's attack roll starts from: the low byte, bits zero to seven. The roll adds the
    /// enemy's frame counter and continues past the field's last value from zero.
    /// </summary>
    internal static readonly RandomSampleField AttackRoll = new(0, 8);
}
