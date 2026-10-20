using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Cartridge values and helpers of <see cref="PolypAnimations"/> that only verification reads.</summary>
internal static class PolypAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PolypProgram[] programs) =>
        programs.SelectMany(program => PolypAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => PolypFrameToolingDefinitions.Spritemap(step.Frame));
}
