using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterProgram;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Cartridge values and helpers of <see cref="KiHunterAnimations"/> that only verification reads.</summary>
internal static class KiHunterAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KiHunterProgram[] programs) =>
        programs.SelectMany(program => KiHunterAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, KiHunterFrame, KiHunterCue>.ShowStep>()
            .Select(step => KiHunterFrameToolingDefinitions.Spritemap(step.Frame));
}
