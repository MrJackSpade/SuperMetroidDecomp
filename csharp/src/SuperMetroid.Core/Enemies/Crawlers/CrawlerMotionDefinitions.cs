namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>How far a crawler probes ahead of its crawl, how it falls and the speed it lands with (#2279).</summary>
internal static class CrawlerMotionDefinitions
{
    /// <summary>The whole pixels a crawler's wall and surface probes reach past its crawl step, in the way it moves.</summary>
    internal const int ProbeMargin = 1;

    /// <summary>The speed a falling crawler gains each update: half a pixel per update.</summary>
    internal static readonly SignedSixteenSixteen FallingGravity = SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The whole-pixel falling speed, positive downward, from which a falling crawler stops falling faster.</summary>
    internal const int FallingSpeedCap = 4;

    /// <summary>
    /// The crawl speed, half a pixel per update on each axis, a crawler placed without one takes after its first landing.
    /// </summary>
    internal static readonly EightEightVector<RoomPixels> DefaultCrawlSpeed =
        new(SignedEightEight.FromParts(0, 128), SignedEightEight.FromParts(0, 128));
}
