using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioProgram;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="LowerNorfairRioAnimations"/> that only verification reads.</summary>
internal static class LowerNorfairRioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params LowerNorfairRioProgram[] programs) =>
        programs.SelectMany(program => LowerNorfairRioAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => LowerNorfairRioFrameDefinitions.Spritemap(step.Frame));
}
