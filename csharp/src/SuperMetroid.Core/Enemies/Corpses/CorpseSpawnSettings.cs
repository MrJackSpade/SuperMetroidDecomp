namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Which authored corpse a dead Zoomer, Ripper or Skree placement is (#1472, #1519).</summary>
/// <param name="Corpse">The corpse, one of its species' <see cref="DeadTourianCorpseDefinitions.Variants"/>.</param>
internal sealed record CorpseSpawnSettings(DeadTourianCorpseDefinition Corpse) : EnemySpawnSettings;
