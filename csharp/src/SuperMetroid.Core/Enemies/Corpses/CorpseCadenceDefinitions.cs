namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The updates on which a rotting corpse's dust sounds (#1606).</summary>
internal static class CorpseCadenceDefinitions
{
    /// <summary>
    /// The enemy passes on which a Torizo, Sidehopper or Tourian corpse's dust cloud requests its liquid-damage sound:
    /// counted on the enemy-pass counter at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence DustSound = new(8, 0);
}
