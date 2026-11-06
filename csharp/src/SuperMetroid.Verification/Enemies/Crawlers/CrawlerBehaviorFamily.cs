using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>The family, definition and species surface family of one crawler behavior family.</summary>
/// <param name="Family">The family.</param>
/// <param name="Definition">The definition.</param>
/// <param name="SpeciesSurfaceFamily">The surface family the species initializer fixes, or none when the entry names it.</param>
internal readonly record struct CrawlerBehaviorFamily(CrawlerAnimationFamily Family, EnemyDefinitionId Definition, CrawlerAnimationFamily? SpeciesSurfaceFamily);
