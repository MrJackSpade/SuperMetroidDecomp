using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Cartridge values and helpers of <see cref="HopperAnimations"/> that only verification reads.</summary>
internal static class HopperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params HopperProgram[] programs) =>
        programs.SelectMany(program => HopperAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<HopperProgram, HopperFrame, HopperCue>.ShowStep>()
            .Select(step => HopperFrameToolingDefinitions.Spritemap(step.Frame));
}
