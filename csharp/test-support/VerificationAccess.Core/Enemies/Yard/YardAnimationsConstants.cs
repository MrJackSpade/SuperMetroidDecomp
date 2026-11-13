using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Yard.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Cartridge values and helpers of <see cref="YardAnimations"/> that only verification reads.</summary>
internal static class YardAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params YardSegment[] programs) =>
        programs.SelectMany(program => YardAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, YardCue>.Frame>()
            .Select(frame => YardFrameToolingDefinitions.Spritemap(frame.Shown));
}
