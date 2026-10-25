namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable native dead-Torizo corpse-rotting configuration record.</summary>
internal readonly record struct DeadTorizoCorpseDefinition(
    int RotTableOffset,
    int EntryCount,
    int WrapOffset);
