using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Cartridge values and helpers of <see cref="MochtroidAnimations"/> that only verification reads.</summary>
internal static class MochtroidAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MochtroidSegment[] programs) =>
        programs.SelectMany(program => MochtroidAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => MochtroidFrameToolingDefinitions.Spritemap(frame.Shown));
}
