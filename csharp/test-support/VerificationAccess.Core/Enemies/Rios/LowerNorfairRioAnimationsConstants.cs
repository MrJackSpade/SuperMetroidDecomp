using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values and helpers of <see cref="LowerNorfairRioAnimations"/> that only verification reads.</summary>
internal static class LowerNorfairRioAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params LowerNorfairRioProgram[] programs) =>
        programs.SelectMany(program => LowerNorfairRioAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue>.ShowStep>()
            .Select(step => LowerNorfairRioFrameToolingDefinitions.Spritemap(step.Frame));
}
