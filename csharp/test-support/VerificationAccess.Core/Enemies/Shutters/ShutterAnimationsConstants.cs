using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Cartridge values and helpers of <see cref="ShutterAnimations"/> that only verification reads.</summary>
internal static class ShutterAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, shown or held (#2276), in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ShutterSegment[] programs) =>
        programs.SelectMany(program => ShutterAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => ShutterFrameToolingDefinitions.Spritemap(frame));
}
