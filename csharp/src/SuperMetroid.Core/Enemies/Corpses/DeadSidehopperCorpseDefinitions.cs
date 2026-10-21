namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Compiled bank-$A9 corpse-rotting metadata for both dead sidehopper variants.</summary>
internal static class DeadSidehopperCorpseDefinitions
{
    /// <summary>
    /// <c>CorpseRottingDefinitions_Sidehopper_Param1_0</c> at <c>$A9:DD68-$A9:DD77</c>,
    /// used by the initially living Shitroid victim.
    /// </summary>
    internal static readonly DeadSidehopperCorpseDefinition InitiallyAlive = new(
        RotTableOffset: 0,
        VramTransfers: new(CorpseRotationTable.Sidehopper, 0),
        EntryCount: 0x0028,
        WrapOffset: 0x0094);

    /// <summary>
    /// <c>CorpseRottingDefinitions_Sidehopper_Param1_2</c> at <c>$A9:DD78-$A9:DD87</c>,
    /// used by the already dead Tourian sidehopper.
    /// </summary>
    internal static readonly DeadSidehopperCorpseDefinition InitiallyDead = new(
        RotTableOffset: 0xa0,
        VramTransfers: new(CorpseRotationTable.Sidehopper, 1),
        EntryCount: 0x0028,
        WrapOffset: 0x0094);

    /// <summary>The palette stage of a corpse that has not started its palette sequence.</summary>
    internal const ushort NoPaletteStage = 0xffff;
}
