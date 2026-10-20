using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Dragon.Artwork;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Cartridge values and helpers of <see cref="DragonAnimations"/> that only verification reads.</summary>
internal static class DragonAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DragonProgram[] programs) =>
        programs.SelectMany(program => DragonAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => DragonFrameToolingDefinitions.Spritemap(step.Frame));
}
