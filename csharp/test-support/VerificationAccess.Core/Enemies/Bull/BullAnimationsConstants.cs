using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>Cartridge values and helpers of <see cref="BullAnimations"/> that only verification reads.</summary>
internal static class BullAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BullProgram[] programs) =>
        programs.SelectMany(program => BullAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<BullProgram, BullFrame, NoAnimationCue>.ShowStep>()
            .Select(step => BullFrameToolingDefinitions.Spritemap(step.Frame));
}
