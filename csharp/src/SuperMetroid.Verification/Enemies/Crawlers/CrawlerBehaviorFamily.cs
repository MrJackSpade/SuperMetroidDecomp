using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>The family and definition of one crawler behavior family.</summary>
/// <param name="Family">The family.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct CrawlerBehaviorFamily(CrawlerAnimationFamily Family, EnemyDefinitionId Definition);
