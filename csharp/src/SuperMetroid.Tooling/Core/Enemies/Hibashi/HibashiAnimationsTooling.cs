using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Hibashi.HibashiProgram;
using SuperMetroid.Core.Enemies.Hibashi.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>Development-tool members of <see cref="HibashiAnimations"/>; never linked by player hosts.</summary>
internal static class HibashiAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params HibashiProgram[] programs) =>
        programs.SelectMany(program => HibashiAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, HibashiFrame, HibashiCue>.ShowStep>()
            .Select(step => HibashiFrameToolingDefinitions.Spritemap(step.Frame));
}
