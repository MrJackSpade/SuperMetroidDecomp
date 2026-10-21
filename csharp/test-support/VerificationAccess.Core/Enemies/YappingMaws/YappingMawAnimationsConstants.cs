using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawProgram;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Cartridge values and helpers of <see cref="YappingMawAnimations"/> that only verification reads.</summary>
internal static class YappingMawAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params YappingMawProgram[] programs) =>
        programs.SelectMany(program => YappingMawAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, YappingMawFrame, YappingMawCue>.ShowStep>()
            .Select(step => YappingMawFrameToolingDefinitions.Spritemap(step.Frame));
}
