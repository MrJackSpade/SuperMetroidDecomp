using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Animals.EscapeDachoraProgram;
using SuperMetroid.Core.Enemies.Animals.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EscapeDachoraAnimations"/>; never linked by player hosts.</summary>
internal static class EscapeDachoraAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EscapeDachoraProgram[] programs) =>
        programs.SelectMany(program => EscapeDachoraAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, EscapeDachoraFrame, EscapeDachoraCue>.ShowStep>()
            .Select(step => EscapeDachoraFrameToolingDefinitions.Spritemap(step.Frame));
}
