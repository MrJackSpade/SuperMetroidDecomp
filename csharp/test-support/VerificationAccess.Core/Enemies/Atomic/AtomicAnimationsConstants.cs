using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>Cartridge values and helpers of <see cref="AtomicAnimations"/> that only verification reads.</summary>
internal static class AtomicAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params AtomicProgram[] programs) =>
        programs.SelectMany(program => AtomicAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => AtomicFrameToolingDefinitions.Spritemap(frame.Shown));
}
