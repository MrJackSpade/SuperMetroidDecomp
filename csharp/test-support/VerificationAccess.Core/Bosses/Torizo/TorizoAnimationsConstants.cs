using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoBodyProgram;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoAnimations"/> that only verification reads.</summary>
internal static class TorizoAnimationsConstants
{
    /// <summary>
    /// The extended spritemaps <paramref name="programs"/> show, in authored order with repeats: the artwork schema's
    /// first-seen frame order.
    /// </summary>
    internal static IEnumerable<ushort> Spritemaps(params TorizoBodyProgram[] programs) =>
        programs.SelectMany(program => TorizoAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, TorizoFrame, TorizoCue>.ShowStep>()
            .Select(step => TorizoFrameDefinitionsConstants.Spritemap(step.Frame));
    /// <summary>Every body program, in authored address order.</summary>
    internal static TorizoBodyProgram[] AllPrograms { get; } = Enum.GetValues<TorizoBodyProgram>();
}
