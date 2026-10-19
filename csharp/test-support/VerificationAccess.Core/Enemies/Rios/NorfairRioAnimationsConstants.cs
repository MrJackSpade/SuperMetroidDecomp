using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.NorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.NorfairRioProgram;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="NorfairRioAnimations"/> that only verification reads.</summary>
internal static class NorfairRioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NorfairRioProgram[] programs) =>
        programs.SelectMany(program => NorfairRioAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => NorfairRioFrameToolingDefinitions.Spritemap(step.Frame));
}
