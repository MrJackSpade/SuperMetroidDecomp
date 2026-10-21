using P = SuperMetroid.Core.Enemies.Animals.EtecoonProgram;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EtecoonAnimations"/>; never linked by player hosts.</summary>
internal static class EtecoonAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EtecoonProgram[] programs) =>
        programs.SelectMany(program => EtecoonAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, EtecoonFrame, NoAnimationCue>.ShowStep>()
            .Select(step => EtecoonFrameToolingDefinitions.Spritemap(step.Frame));
}
