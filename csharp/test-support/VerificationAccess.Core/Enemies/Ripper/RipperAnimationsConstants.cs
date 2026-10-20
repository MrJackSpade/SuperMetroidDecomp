using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>Cartridge values and helpers of <see cref="RipperAnimations"/> that only verification reads.</summary>
internal static class RipperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RipperProgram[] programs) =>
        programs.SelectMany(program => RipperAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<RipperProgram, RipperFrame, NoAnimationCue>.ShowStep>()
            .Select(step => RipperFrameToolingDefinitions.Spritemap(step.Frame));
}
