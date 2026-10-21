using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProgram;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Development-tool members of <see cref="NuclearWaffleAnimations"/>; never linked by player hosts.</summary>
internal static class NuclearWaffleAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NuclearWaffleProgram[] programs) =>
        programs.SelectMany(program => NuclearWaffleAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, NuclearWaffleFrame, NoAnimationCue>.ShowStep>()
            .Select(step => NuclearWaffleFrameToolingDefinitions.Spritemap(step.Frame));
}
