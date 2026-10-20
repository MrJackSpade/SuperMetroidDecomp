using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Waver.Artwork;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Cartridge values and helpers of <see cref="WaverAnimations"/> that only verification reads.</summary>
internal static class WaverAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WaverProgram[] programs) =>
        programs.SelectMany(program => WaverAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<WaverProgram, WaverFrame, WaverCue>.ShowStep>()
            .Select(step => WaverFrameToolingDefinitions.Spritemap(step.Frame));
}
