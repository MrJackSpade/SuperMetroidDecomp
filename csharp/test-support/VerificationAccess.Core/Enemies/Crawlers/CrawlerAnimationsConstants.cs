using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values and helpers of <see cref="CrawlerAnimations"/> that only verification reads.</summary>
internal static class CrawlerAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params CrawlerSegment[] programs) =>
        programs.SelectMany(program => CrawlerAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, CrawlerCue>.Frame>()
            .Select(frame => CrawlerFrameToolingDefinitions.Spritemap(frame.Shown));
}
