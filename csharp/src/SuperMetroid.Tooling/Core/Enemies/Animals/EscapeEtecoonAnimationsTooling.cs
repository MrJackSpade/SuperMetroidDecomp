using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonProgram;
using SuperMetroid.Core.Enemies.Animals.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EscapeEtecoonAnimations"/>; never linked by player hosts.</summary>
internal static class EscapeEtecoonAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EscapeEtecoonProgram[] programs) =>
        programs.SelectMany(program => EscapeEtecoonAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, EscapeEtecoonFrame, EscapeEtecoonCue>.ShowStep>()
            .Select(step => EscapeEtecoonFrameToolingDefinitions.Spritemap(step.Frame));
}
