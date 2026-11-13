using SuperMetroid.Core.Enemies.Kzan.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>Cartridge values and helpers of <see cref="KzanAnimations"/> that only verification reads.</summary>
internal static class KzanAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KzanSegment[] programs) =>
        programs.SelectMany(program => KzanAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => KzanFrameToolingDefinitions.Spritemap(frame.Shown));
}
