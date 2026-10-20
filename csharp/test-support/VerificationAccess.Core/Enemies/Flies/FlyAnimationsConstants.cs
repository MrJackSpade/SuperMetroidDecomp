using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>Cartridge values and helpers of <see cref="FlyAnimations"/> that only verification reads.</summary>
internal static class FlyAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FlyProgram[] programs) =>
        programs.SelectMany(program => FlyAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => FlyFrameToolingDefinitions.Spritemap(step.Frame));
}
