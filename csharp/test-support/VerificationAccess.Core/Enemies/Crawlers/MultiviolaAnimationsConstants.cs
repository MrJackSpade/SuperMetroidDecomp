using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values and helpers of <see cref="MultiviolaAnimations"/> that only verification reads.</summary>
internal static class MultiviolaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MultiviolaSegment[] programs) =>
        programs.SelectMany(program => MultiviolaAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame>()
            .Select(frame => MultiviolaFrameToolingDefinitions.Spritemap(frame.Shown));
}
