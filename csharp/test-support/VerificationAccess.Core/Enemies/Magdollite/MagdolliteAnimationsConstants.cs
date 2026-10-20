using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Cartridge values and helpers of <see cref="MagdolliteAnimations"/> that only verification reads.</summary>
internal static class MagdolliteAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MagdolliteProgram[] programs) =>
        programs.SelectMany(program => MagdolliteAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.ShowStep>()
            .Select(step => MagdolliteFrameToolingDefinitions.Spritemap(step.Frame));
}
