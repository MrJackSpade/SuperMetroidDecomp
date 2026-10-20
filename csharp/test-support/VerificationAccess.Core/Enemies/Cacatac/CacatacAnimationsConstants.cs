using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cartridge values and helpers of <see cref="CacatacAnimations"/> that only verification reads.</summary>
internal static class CacatacAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params CacatacProgram[] programs) =>
        programs.SelectMany(program => CacatacAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue>.ShowStep>()
            .Select(step => CacatacFrameToolingDefinitions.Spritemap(step.Frame));
}
