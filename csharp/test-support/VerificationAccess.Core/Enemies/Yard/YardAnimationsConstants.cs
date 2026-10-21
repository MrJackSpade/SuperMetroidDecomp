using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Yard.YardProgram;
using SuperMetroid.Core.Enemies.Yard.Artwork;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Cartridge values and helpers of <see cref="YardAnimations"/> that only verification reads.</summary>
internal static class YardAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params YardProgram[] programs) =>
        programs.SelectMany(program => YardAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, YardFrame, YardCue>.ShowStep>()
            .Select(step => YardFrameToolingDefinitions.Spritemap(step.Frame));
}
