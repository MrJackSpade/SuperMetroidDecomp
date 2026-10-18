using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Beetom.BeetomCueKind;
using P = SuperMetroid.Core.Enemies.Beetom.BeetomProgram;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>Cartridge values and helpers of <see cref="BeetomAnimations"/> that only verification reads.</summary>
internal static class BeetomAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BeetomProgram[] programs) =>
        programs.SelectMany(program => BeetomAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => BeetomFrameToolingDefinitions.Spritemap(step.Frame));
}
