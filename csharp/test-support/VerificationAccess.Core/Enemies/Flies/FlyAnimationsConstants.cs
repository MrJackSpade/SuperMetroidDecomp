using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>Cartridge values and helpers of <see cref="FlyAnimations"/> that only verification reads.</summary>
internal static class FlyAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FlySegment[] programs) =>
        programs.SelectMany(program => FlyAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => FlyFrameToolingDefinitions.Spritemap(frame.Shown));
}
