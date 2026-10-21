namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>A dead monster's VRAM transfer list: the species' list for one of its corpse variants.</summary>
/// <param name="Species">The species whose corpse the list uploads.</param>
/// <param name="Variant">Which of the species' corpse variants it is.</param>
internal readonly record struct CorpseTransferTable(CorpseRotationTable Species, int Variant);
