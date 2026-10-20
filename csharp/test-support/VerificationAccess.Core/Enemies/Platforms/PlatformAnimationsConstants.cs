using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Platforms.Artwork;

namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Cartridge values and helpers of <see cref="PlatformAnimations"/> that only verification reads.</summary>
internal static class PlatformAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PlatformProgram[] programs) =>
        programs.SelectMany(program => PlatformAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => PlatformFrameToolingDefinitions.Spritemap(step.Frame));
}
