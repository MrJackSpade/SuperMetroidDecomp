using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Kraid.KraidPartCue;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Development-tool members of <see cref="KraidAnimations"/>; never linked by player hosts.</summary>
internal static class KraidAnimationsTooling
{
    /// <summary>The spritemaps the <paramref name="programs"/>' segments show, in beat order, repeats included.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KraidPartSegment[] programs) =>
        programs.SelectMany(program => KraidAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<KraidPartFrame>()
            .Select(frame => KraidPartFrameToolingDefinitions.Spritemap(frame));
}
