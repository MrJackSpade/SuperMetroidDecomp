namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native dead-monster initialization record.</summary>
internal readonly record struct DeadTourianCorpseDefinition(
    DeadTourianProgram InitialProgram,
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort EntryCount,
    ushort WrapOffset);
