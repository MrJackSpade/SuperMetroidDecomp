namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native corpse-rotting configuration record.</summary>
internal readonly record struct DeadSidehopperCorpseDefinition(
    int RotTableOffset,
    RottingCorpseKind Kind,
    int EntryCount,
    int WrapOffset);
