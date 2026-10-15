using P = SuperMetroid.Core.Enemies.Ripper.RipperProgram;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>Cartridge values and helpers of <see cref="RipperAnimations"/> that only verification reads.</summary>
internal static class RipperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RipperProgram[] programs) =>
        programs.SelectMany(program => RipperAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => RipperFrameDefinitions.Spritemap(step.Frame));
}
