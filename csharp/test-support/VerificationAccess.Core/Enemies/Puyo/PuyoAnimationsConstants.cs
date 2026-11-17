using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Cartridge values and helpers of <see cref="PuyoAnimations"/> that only verification reads.</summary>
internal static class PuyoAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, shown or held (#2276), in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PuyoSegment[] programs) =>
        programs.SelectMany(program => PuyoAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => PuyoFrameToolingDefinitions.Spritemap(frame));
}
