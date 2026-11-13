using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Cartridge values and helpers of <see cref="KiHunterAnimations"/> that only verification reads.</summary>
internal static class KiHunterAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KiHunterSegment[] programs) =>
        programs.SelectMany(program => KiHunterAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, KiHunterCue>.Frame>()
            .Select(frame => KiHunterFrameToolingDefinitions.Spritemap(frame.Shown));
}
