using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Corpses.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values and helpers of <see cref="DeadTourianAnimations"/> that only verification reads.</summary>
internal static class DeadTourianAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DeadTourianProgram[] programs) =>
        programs.SelectMany(program => DeadTourianAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => DeadTourianFrameToolingDefinitions.Spritemap(step.Frame));
}
