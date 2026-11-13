namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One cue of the crawlers' animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct CrawlerCue(CrawlerCueKind Kind, CrawlerEnemyFunction Function = default);
