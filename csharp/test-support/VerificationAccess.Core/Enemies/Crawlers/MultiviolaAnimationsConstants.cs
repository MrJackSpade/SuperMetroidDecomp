using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values and helpers of <see cref="MultiviolaAnimations"/> that only verification reads.</summary>
internal static class MultiviolaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MultiviolaProgram[] programs) =>
        programs.SelectMany(program => MultiviolaAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<MultiviolaFrame, NoAnimationCue>.Frame>()
            .Select(frame => MultiviolaFrameToolingDefinitions.Spritemap(frame.Shown));
}
