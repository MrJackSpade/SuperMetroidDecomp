using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Cartridge values and helpers of <see cref="PipeBugAnimations"/> that only verification reads.</summary>
internal static class PipeBugAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PipeBugProgram[] programs) =>
        programs.SelectMany(program => PipeBugAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => PipeBugFrameToolingDefinitions.Spritemap(frame.Shown));
}
