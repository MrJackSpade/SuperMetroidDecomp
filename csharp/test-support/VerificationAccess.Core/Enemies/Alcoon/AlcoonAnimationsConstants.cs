using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Cartridge values and helpers of <see cref="AlcoonAnimations"/> that only verification reads.</summary>
internal static class AlcoonAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params AlcoonSegment[] programs) =>
        programs.SelectMany(program => AlcoonAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => AlcoonFrameToolingDefinitions.Spritemap(frame));
}
