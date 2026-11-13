using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cartridge values and helpers of <see cref="CacatacAnimations"/> that only verification reads.</summary>
internal static class CacatacAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params CacatacSegment[] programs) =>
        programs.SelectMany(program => CacatacAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, CacatacCue>.Frame>()
            .Select(frame => CacatacFrameToolingDefinitions.Spritemap(frame.Shown));
}
