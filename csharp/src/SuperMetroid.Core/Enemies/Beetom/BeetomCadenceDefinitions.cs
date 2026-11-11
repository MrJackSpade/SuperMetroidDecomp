namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The updates on which a Beetom latched onto Samus drains her and sounds the drain (#1606).</summary>
internal static class BeetomCadenceDefinitions
{
    /// <summary>
    /// The enemy passes on which a latched Beetom queues the energy-drain sound: every eighth pass of the enemy-pass
    /// counter, on its last place, while Samus has 30 energy or more.
    /// </summary>
    internal static readonly UpdateCadence DrainSoundEveryEighthEnemyPass = new(8, 7);

    /// <summary>
    /// The updates on which a latched Beetom takes energy from Samus: every 64th update of its own update count, on the
    /// period's last place.
    /// </summary>
    internal static readonly UpdateCadence DrainDamageEverySixtyFourthUpdate = new(64, 63);
}
