using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperProgram;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>Cartridge values and helpers of <see cref="NorfairLavaJumperAnimations"/> that only verification reads.</summary>
internal static class NorfairLavaJumperAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NorfairLavaJumperProgram[] programs) =>
        programs.SelectMany(program => NorfairLavaJumperAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, NorfairLavaJumperFrame, NorfairLavaJumperCue>.ShowStep>()
            .Select(step => NorfairLavaJumperFrameToolingDefinitions.Spritemap(step.Frame));
}
