using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using P = SuperMetroid.Core.Bosses.Kraid.KraidPartProgram;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Development-tool members of <see cref="KraidAnimations"/>; never linked by player hosts.</summary>
internal static class KraidAnimationsTooling
{
    /// <summary>The spritemaps the <paramref name="programs"/> show, in step order, repeats included.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KraidPartProgram[] programs) =>
        programs.SelectMany(program => KraidAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, KraidPartFrame, K>.ShowStep>()
            .Select(step => KraidPartFrameToolingDefinitions.Spritemap(step.Frame));
}
