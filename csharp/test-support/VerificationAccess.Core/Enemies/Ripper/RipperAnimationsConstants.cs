using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>Cartridge values and helpers of <see cref="RipperAnimations"/> that only verification reads.</summary>
internal static class RipperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RipperSegment[] programs) =>
        programs.SelectMany(program => RipperAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => RipperFrameToolingDefinitions.Spritemap(frame.Shown));
}
