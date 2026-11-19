using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="GerutaAnimations"/> that only verification reads.</summary>
internal static class GerutaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params GerutaSegment[] programs) =>
        programs.SelectMany(program => GerutaAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => GerutaFrameToolingDefinitions.Spritemap(frame));
}
