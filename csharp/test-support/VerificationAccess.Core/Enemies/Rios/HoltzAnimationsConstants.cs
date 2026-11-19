using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="HoltzAnimations"/> that only verification reads.</summary>
internal static class HoltzAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params HoltzSegment[] programs) =>
        programs.SelectMany(program => HoltzAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => HoltzFrameToolingDefinitions.Spritemap(frame));
}
