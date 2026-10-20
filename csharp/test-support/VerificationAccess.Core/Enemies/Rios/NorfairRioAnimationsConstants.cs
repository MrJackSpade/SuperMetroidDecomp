using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="NorfairRioAnimations"/> that only verification reads.</summary>
internal static class NorfairRioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NorfairRioProgram[] programs) =>
        programs.SelectMany(program => NorfairRioAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<NorfairRioProgram, NorfairRioFrame, NorfairRioCue>.ShowStep>()
            .Select(step => NorfairRioFrameToolingDefinitions.Spritemap(step.Frame));
}
