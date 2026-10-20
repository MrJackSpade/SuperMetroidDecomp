using SuperMetroid.Core.Enemies.Kzan.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>Cartridge values and helpers of <see cref="KzanAnimations"/> that only verification reads.</summary>
internal static class KzanAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KzanProgram[] programs) =>
        programs.SelectMany(program => KzanAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<KzanProgram, KzanFrame, NoAnimationCue>.ShowStep>()
            .Select(step => KzanFrameToolingDefinitions.Spritemap(step.Frame));
}
