using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Zoa.Artwork;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Cartridge values and helpers of <see cref="ZoaAnimations"/> that only verification reads.</summary>
internal static class ZoaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ZoaProgram[] programs) =>
        programs.SelectMany(program => ZoaAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => ZoaFrameToolingDefinitions.Spritemap(step.Frame));
}
