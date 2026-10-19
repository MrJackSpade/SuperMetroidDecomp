using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hoppers.HopperCueKind;
using P = SuperMetroid.Core.Enemies.Hoppers.HopperProgram;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Cartridge values and helpers of <see cref="HopperAnimations"/> that only verification reads.</summary>
internal static class HopperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params HopperProgram[] programs) =>
        programs.SelectMany(program => HopperAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => HopperFrameToolingDefinitions.Spritemap(step.Frame));
}
