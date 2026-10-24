namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native dead-monster initialization record.</summary>
internal readonly record struct DeadTourianCorpseDefinition(
    DeadTourianProgram InitialProgram,
    int RotTableOffset,
    RottingCorpseKind Kind,
    ushort EntryCount,
    ushort WrapOffset);
