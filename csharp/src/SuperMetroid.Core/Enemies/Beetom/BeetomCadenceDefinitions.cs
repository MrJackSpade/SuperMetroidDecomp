namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The updates on which a Beetom latched onto Samus drains her and sounds the drain (#1606).</summary>
internal static class BeetomCadenceDefinitions
{
    /// <summary>
    /// The enemy passes on which a latched Beetom queues the energy-drain sound: the last place of each period of the
    /// enemy-pass counter, while Samus has at least <see cref="DrainSoundLeastHealth"/>.
    /// </summary>
    internal static readonly UpdateCadence DrainSoundEveryEighthEnemyPass = new(8, 7);

    /// <summary>The least energy Samus has for a latched Beetom's drain to sound.</summary>
    internal const int DrainSoundLeastHealth = 30;

    /// <summary>
    /// The updates on which a latched Beetom takes energy from Samus: the last place of each period of its own update
    /// count.
    /// </summary>
    internal static readonly UpdateCadence DrainDamageEverySixtyFourthUpdate = new(64, 63);
}
