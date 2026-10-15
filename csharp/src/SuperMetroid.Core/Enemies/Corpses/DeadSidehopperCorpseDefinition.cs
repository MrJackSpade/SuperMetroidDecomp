namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native corpse-rotting configuration record.</summary>
internal readonly record struct DeadSidehopperCorpseDefinition(
    ushort ConfigurationPointer,
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort CopyFunction,
    ushort MoveFunction,
    ushort EntryCount,
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
