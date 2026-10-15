using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Choot.ChootCueKind;
using P = SuperMetroid.Core.Enemies.Choot.ChootProgram;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Cartridge values and helpers of <see cref="ChootAnimations"/> that only verification reads.</summary>
internal static class ChootAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ChootProgram[] programs) =>
        programs.SelectMany(program => ChootAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => ChootFrameDefinitions.Spritemap(step.Frame));
}
