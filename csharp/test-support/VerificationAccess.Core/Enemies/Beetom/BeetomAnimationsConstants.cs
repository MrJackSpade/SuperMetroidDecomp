using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>Cartridge values and helpers of <see cref="BeetomAnimations"/> that only verification reads.</summary>
internal static class BeetomAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, timed or held, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BeetomSegment[] programs) =>
        programs.SelectMany(program => BeetomAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => BeetomFrameToolingDefinitions.Spritemap(frame));
}
