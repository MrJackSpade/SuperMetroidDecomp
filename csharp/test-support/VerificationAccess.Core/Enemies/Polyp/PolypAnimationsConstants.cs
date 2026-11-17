using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Cartridge values and helpers of <see cref="PolypAnimations"/> that only verification reads.</summary>
internal static class PolypAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, shown or held (#2276), in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PolypSegment[] programs) =>
        programs.SelectMany(program => PolypAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => PolypFrameToolingDefinitions.Spritemap(frame));
}
