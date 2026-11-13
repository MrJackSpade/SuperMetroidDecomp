using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>Cartridge values and helpers of <see cref="BullAnimations"/> that only verification reads.</summary>
internal static class BullAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BullProgram[] programs) =>
        programs.SelectMany(program => BullAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => BullFrameToolingDefinitions.Spritemap(frame.Shown));
}
