using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Choot.Artwork;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Cartridge values and helpers of <see cref="ChootAnimations"/> that only verification reads.</summary>
internal static class ChootAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ChootProgram[] programs) =>
        programs.SelectMany(program => ChootAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<ChootProgram, ChootFrame, ChootCue>.ShowStep>()
            .Select(step => ChootFrameToolingDefinitions.Spritemap(step.Frame));
}
