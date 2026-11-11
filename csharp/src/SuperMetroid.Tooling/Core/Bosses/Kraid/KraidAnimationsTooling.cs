using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using P = SuperMetroid.Core.Bosses.Kraid.KraidPartProgram;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Development-tool members of <see cref="KraidAnimations"/>; never linked by player hosts.</summary>
internal static class KraidAnimationsTooling
{
    /// <summary>The spritemaps the <paramref name="programs"/>' segments show, in beat order, repeats included.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KraidPartProgram[] programs) =>
        programs.SelectMany(program => KraidAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<KraidPartFrame, K>.Frame>()
            .Select(frame => KraidPartFrameToolingDefinitions.Spritemap(frame.Shown));
}
