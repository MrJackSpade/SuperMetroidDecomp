namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One cue of the crawlers' animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Action">For an action cue: the crawling or falling behaviour it installs.</param>
internal readonly record struct CrawlerCue(CrawlerCueKind Kind, CrawlerAction Action);
