using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Cartridge values and helpers of <see cref="MaridiaLargeSnailAnimations"/> that only verification reads.</summary>
internal static class MaridiaLargeSnailAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MaridiaLargeSnailSegment[] programs) =>
        programs.SelectMany(program => MaridiaLargeSnailAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => MaridiaLargeSnailFrameToolingDefinitions.Spritemap(frame));
}
