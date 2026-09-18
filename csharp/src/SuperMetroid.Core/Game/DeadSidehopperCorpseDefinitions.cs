namespace SuperMetroid.Core.Game;

/// <summary>Compiled bank-$A9 corpse-rotting metadata for both dead sidehopper variants.</summary>
internal static class DeadSidehopperCorpseDefinitions
{
    /// <summary>
    /// <c>CorpseRottingDefinitions_Sidehopper_Param1_0</c> at <c>$A9:DD68-$A9:DD77</c>,
    /// used by the initially living Shitroid victim.
    /// </summary>
    internal static readonly DeadSidehopperCorpseDefinition InitiallyAlive = new(
        ConfigurationPointer: 0xdd68,
        RottingTablePointer: 0x9000,
        VramTransferPointer: 0xe0e0,
        CopyFunction: 0xe4f5,
        MoveFunction: 0xe468,
        EntryCount: 0x0028,
        GraphicsInitializationFunction: 0xdec1,
        RotationTablePointer: 0xe240,
        FinishFunction: 0xdc08,
        WrapOffset: 0x0094);

    /// <summary>
    /// <c>CorpseRottingDefinitions_Sidehopper_Param1_2</c> at <c>$A9:DD78-$A9:DD87</c>,
    /// used by the already dead Tourian sidehopper.
    /// </summary>
    internal static readonly DeadSidehopperCorpseDefinition InitiallyDead = new(
        ConfigurationPointer: 0xdd78,
        RottingTablePointer: 0x90a0,
        VramTransferPointer: 0xe10a,
        CopyFunction: 0xe5f6,
        MoveFunction: 0xe564,
        EntryCount: 0x0028,
        GraphicsInitializationFunction: 0xdf08,
        RotationTablePointer: 0xe240,
        FinishFunction: 0xdc08,
        WrapOffset: 0x0094);
}

/// <summary>One immutable native corpse-rotting configuration record.</summary>
internal readonly record struct DeadSidehopperCorpseDefinition(
    ushort ConfigurationPointer,
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort CopyFunction,
    ushort MoveFunction,
    ushort EntryCount,
    ushort GraphicsInitializationFunction,
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
