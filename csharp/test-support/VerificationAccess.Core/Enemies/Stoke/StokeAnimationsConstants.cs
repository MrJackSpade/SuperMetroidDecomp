using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Stoke.StokeCueKind;
using P = SuperMetroid.Core.Enemies.Stoke.StokeProgram;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Cartridge values and helpers of <see cref="StokeAnimations"/> that only verification reads.</summary>
internal static class StokeAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params StokeProgram[] programs) =>
        programs.SelectMany(program => StokeAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => StokeFrameToolingDefinitions.Spritemap(step.Frame));
}
