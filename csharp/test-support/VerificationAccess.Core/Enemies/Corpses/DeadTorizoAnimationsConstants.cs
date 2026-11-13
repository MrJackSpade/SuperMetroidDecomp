using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values and helpers of <see cref="DeadTorizoAnimations"/> that only verification reads.</summary>
internal static class DeadTorizoAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DeadTorizoSegment[] programs) =>
        programs.SelectMany(program => DeadTorizoAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => DeadTorizoFrameToolingDefinitions.Spritemap(frame.Shown));
}
