using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Skultera.SkulteraCueKind;
using P = SuperMetroid.Core.Enemies.Skultera.SkulteraProgram;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>Cartridge values and helpers of <see cref="SkulteraAnimations"/> that only verification reads.</summary>
internal static class SkulteraAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SkulteraProgram[] programs) =>
        programs.SelectMany(program => SkulteraAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => SkulteraFrameDefinitions.Spritemap(step.Frame));
}
