namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The updates on which a rotting corpse's dust sounds (#1606).</summary>
internal static class CorpseCadenceDefinitions
{
    /// <summary>
    /// The enemy passes on which a Torizo, Sidehopper or Tourian corpse's dust cloud requests its liquid-damage sound:
    /// every eighth pass of the enemy-pass counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence DustSoundEveryEighthEnemyPass = new(8, 0);
}
