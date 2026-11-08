namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How both dead sidehopper variants' corpses rot.</summary>
internal static class DeadSidehopperCorpseDefinitions
{
    /// <summary>The initially living Shitroid victim's corpse.</summary>
    internal static readonly DeadSidehopperCorpseDefinition InitiallyAlive = new(
        Kind: new(RottingCorpseSpecies.Sidehopper, 0),
        RowCount: 40);

    /// <summary>The already dead Tourian sidehopper's corpse.</summary>
    internal static readonly DeadSidehopperCorpseDefinition InitiallyDead = new(
        Kind: new(RottingCorpseSpecies.Sidehopper, 1),
        RowCount: 40);
}
