namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native corpse-rotting configuration record.</summary>
internal readonly record struct DeadSidehopperCorpseDefinition(
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort EntryCount,
    ushort WrapOffset);
