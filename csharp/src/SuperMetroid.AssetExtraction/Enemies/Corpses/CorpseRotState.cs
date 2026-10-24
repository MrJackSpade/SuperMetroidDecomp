using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>One rot a corpse's frames can show (#1406): the rot the game selects, and the step each of the corpse's rows shows in it.</summary>
/// <param name="View">The rot as the game selects it, which names its pictures.</param>
/// <param name="Steps">The rot step each row shows, or none while it shows the graphics set's characters.</param>
internal sealed record CorpseRotState(CorpseRotView View, IReadOnlyDictionary<CorpseRotRow, int?> Steps);
