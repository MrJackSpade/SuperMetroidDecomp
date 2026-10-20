using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Boyon.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>Cartridge values and helpers of <see cref="BoyonAnimations"/> that only verification reads.</summary>
internal static class BoyonAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BoyonProgram[] programs) =>
        programs.SelectMany(program => BoyonAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue>.ShowStep>()
            .Select(step => BoyonFrameToolingDefinitions.Spritemap(step.Frame));
}
