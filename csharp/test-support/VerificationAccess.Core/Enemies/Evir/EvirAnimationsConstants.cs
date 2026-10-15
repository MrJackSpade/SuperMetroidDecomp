using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Evir.EvirCueKind;
using P = SuperMetroid.Core.Enemies.Evir.EvirProgram;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Cartridge values and helpers of <see cref="EvirAnimations"/> that only verification reads.</summary>
internal static class EvirAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EvirProgram[] programs) =>
        programs.SelectMany(program => EvirAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => EvirFrameDefinitions.Spritemap(step.Frame));
}
