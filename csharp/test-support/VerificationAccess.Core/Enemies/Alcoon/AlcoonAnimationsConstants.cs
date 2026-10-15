using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Alcoon.AlcoonCueKind;
using P = SuperMetroid.Core.Enemies.Alcoon.AlcoonProgram;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Cartridge values and helpers of <see cref="AlcoonAnimations"/> that only verification reads.</summary>
internal static class AlcoonAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params AlcoonProgram[] programs) =>
        programs.SelectMany(program => AlcoonAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => AlcoonFrameDefinitions.Spritemap(step.Frame));
}
