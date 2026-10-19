using P = SuperMetroid.Core.Enemies.PipeBugs.PipeBugProgram;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Cartridge values and helpers of <see cref="PipeBugAnimations"/> that only verification reads.</summary>
internal static class PipeBugAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PipeBugProgram[] programs) =>
        programs.SelectMany(program => PipeBugAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => PipeBugFrameToolingDefinitions.Spritemap(step.Frame));
}
