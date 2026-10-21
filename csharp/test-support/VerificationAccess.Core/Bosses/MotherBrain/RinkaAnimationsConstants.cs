using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.MotherBrain.RinkaProgram;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="RinkaAnimations"/> that only verification reads.</summary>
internal static class RinkaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RinkaProgram[] programs) =>
        programs.SelectMany(program => RinkaAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, RinkaFrame, RinkaCue>.ShowStep>()
            .Select(step => RinkaFrameToolingDefinitions.Spritemap(step.Frame));
}
