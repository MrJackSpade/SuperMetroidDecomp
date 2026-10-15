namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>What a cue of the crawlers' programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum CrawlerCueKind
{
    /// <summary>Sets the movement function main AI runs (<c>FunctionInY</c>, $A3:E660).</summary>
    SetFunction,
}
