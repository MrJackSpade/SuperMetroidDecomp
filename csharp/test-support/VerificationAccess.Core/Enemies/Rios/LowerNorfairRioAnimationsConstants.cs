using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="LowerNorfairRioAnimations"/> that only verification reads.</summary>
internal static class LowerNorfairRioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params LowerNorfairRioSegment[] programs) =>
        programs.SelectMany(program => LowerNorfairRioAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => LowerNorfairRioFrameToolingDefinitions.Spritemap(frame));
}
