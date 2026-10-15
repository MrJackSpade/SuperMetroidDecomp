namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native dead-Torizo corpse-rotting configuration record.</summary>
internal readonly record struct DeadTorizoCorpseDefinition(
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort CopyFunction,
    ushort MoveFunction,
    ushort EntryCount,
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
