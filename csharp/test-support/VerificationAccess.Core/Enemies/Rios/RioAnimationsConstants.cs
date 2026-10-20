using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="RioAnimations"/> that only verification reads.</summary>
internal static class RioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RioProgram[] programs) =>
        programs.SelectMany(program => RioAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => RioFrameToolingDefinitions.Spritemap(step.Frame));
}
