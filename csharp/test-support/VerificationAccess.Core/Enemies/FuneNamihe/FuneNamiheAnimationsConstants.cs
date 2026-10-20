using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Cartridge values and helpers of <see cref="FuneNamiheAnimations"/> that only verification reads.</summary>
internal static class FuneNamiheAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FuneNamiheProgram[] programs) =>
        programs.SelectMany(program => FuneNamiheAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue>.ShowStep>()
            .Select(step => FuneNamiheFrameToolingDefinitions.Spritemap(step.Frame));
}
