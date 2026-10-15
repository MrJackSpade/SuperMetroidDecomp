namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native dead-monster initialization record.</summary>
internal readonly record struct DeadTourianCorpseDefinition(
    DeadTourianProgram InitialProgram,
    ushort ConfigurationPointer,
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort CopyFunction,
    ushort MoveFunction,
    ushort EntryCount,
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
