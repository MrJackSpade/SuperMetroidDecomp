namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One cue of the crawlers' animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Action">The cue's Action operand.</param>
internal readonly record struct CrawlerCue(CrawlerCueKind Kind, CrawlerAction Action = default);
