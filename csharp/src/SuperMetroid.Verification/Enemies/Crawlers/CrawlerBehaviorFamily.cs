using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>The family, definition and species offset of one crawler behavior family.</summary>
/// <param name="Family">The family.</param>
/// <param name="Definition">The definition.</param>
/// <param name="SpeciesOffset">The species offset.</param>
internal readonly record struct CrawlerBehaviorFamily(CrawlerAnimationFamily Family, EnemyDefinitionId Definition, ushort? SpeciesOffset);
