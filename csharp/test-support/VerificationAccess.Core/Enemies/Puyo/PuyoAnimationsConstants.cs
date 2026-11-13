using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Cartridge values and helpers of <see cref="PuyoAnimations"/> that only verification reads.</summary>
internal static class PuyoAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PuyoSegment[] programs) =>
        programs.SelectMany(program => PuyoAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => PuyoFrameToolingDefinitions.Spritemap(frame.Shown));
}
