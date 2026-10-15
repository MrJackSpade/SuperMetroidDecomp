namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One cue of the crawlers' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct CrawlerCue(CrawlerCueKind Kind, CrawlerEnemyFunction Function = default);
