using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoStatueProgram;
using SuperMetroid.Core.Enemies.Chozo.Artwork;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Cartridge values and helpers of <see cref="ChozoStatueAnimations"/> that only verification reads.</summary>
internal static class ChozoStatueAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ChozoStatueProgram[] programs) =>
        programs.SelectMany(program => ChozoStatueAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, ChozoStatueFrame, ChozoStatueCue>.ShowStep>()
            .Select(step => ChozoStatueFrameToolingDefinitions.Spritemap(step.Frame));
}
