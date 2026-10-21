using P = SuperMetroid.Core.Enemies.Crawlers.MultiviolaProgram;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values and helpers of <see cref="MultiviolaAnimations"/> that only verification reads.</summary>
internal static class MultiviolaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MultiviolaProgram[] programs) =>
        programs.SelectMany(program => MultiviolaAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, MultiviolaFrame, NoAnimationCue>.ShowStep>()
            .Select(step => MultiviolaFrameToolingDefinitions.Spritemap(step.Frame));
}
