using P = SuperMetroid.Core.Enemies.Sbug.SbugProgram;
using SuperMetroid.Core.Enemies.Sbug.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Cartridge values and helpers of <see cref="SbugAnimations"/> that only verification reads.</summary>
internal static class SbugAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SbugProgram[] programs) =>
        programs.SelectMany(program => SbugAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => SbugFrameDefinitions.Spritemap(step.Frame));
}
