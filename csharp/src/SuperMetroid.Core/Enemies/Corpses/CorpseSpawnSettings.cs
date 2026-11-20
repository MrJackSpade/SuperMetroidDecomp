namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Which authored corpse a dead Zoomer, Ripper or Skree placement is (#1472, #1519, #1810).</summary>
/// <param name="Corpse">The corpse, one of <see cref="DeadTourianCorpseDefinitions"/>.</param>
internal sealed record CorpseSpawnSettings(DeadTourianCorpseDefinition Corpse) : EnemySpawnSettings;
