using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Cartridge values and helpers of <see cref="MorphBallEyeAnimations"/> that only verification reads.</summary>
internal static class MorphBallEyeAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MorphBallEyeSegment[] programs) =>
        programs.SelectMany(program => MorphBallEyeAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => MorphBallEyeFrameToolingDefinitions.Spritemap(frame.Shown));
}
