using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.MamaTurtles.MamaTurtleProgram;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Development-tool members of <see cref="MamaTurtleAnimations"/>; never linked by player hosts.</summary>
internal static class MamaTurtleAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MamaTurtleProgram[] programs) =>
        programs.SelectMany(program => MamaTurtleAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, MamaTurtleFrame, MamaTurtleCue>.ShowStep>()
            .Select(step => MamaTurtleFrameToolingDefinitions.Spritemap(step.Frame));
}
