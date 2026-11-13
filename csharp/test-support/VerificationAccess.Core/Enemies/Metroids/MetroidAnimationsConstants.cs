using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Cartridge values and helpers of <see cref="MetroidAnimations"/> that only verification reads.</summary>
internal static class MetroidAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MetroidSegment[] programs) =>
        programs.SelectMany(program => MetroidAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, MetroidCue>.Frame>()
            .Select(frame => MetroidFrameToolingDefinitions.Spritemap(frame.Shown));
}
