using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Cartridge values and helpers of <see cref="MaridiaLargeSnailAnimations"/> that only verification reads.</summary>
internal static class MaridiaLargeSnailAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MaridiaLargeSnailProgram[] programs) =>
        programs.SelectMany(program => MaridiaLargeSnailAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MaridiaLargeSnailFrameToolingDefinitions.Spritemap(step.Frame));
}
