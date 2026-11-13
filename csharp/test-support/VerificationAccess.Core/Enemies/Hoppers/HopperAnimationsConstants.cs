using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Cartridge values and helpers of <see cref="HopperAnimations"/> that only verification reads.</summary>
internal static class HopperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params HopperSegment[] programs) =>
        programs.SelectMany(program => HopperAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, HopperCue>.Frame>()
            .Select(frame => HopperFrameToolingDefinitions.Spritemap(frame.Shown));
}
