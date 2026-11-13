using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>Cartridge values and helpers of <see cref="BeetomAnimations"/> that only verification reads.</summary>
internal static class BeetomAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BeetomSegment[] programs) =>
        programs.SelectMany(program => BeetomAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, BeetomCue>.Frame>()
            .Select(frame => BeetomFrameToolingDefinitions.Spritemap(frame.Shown));
}
