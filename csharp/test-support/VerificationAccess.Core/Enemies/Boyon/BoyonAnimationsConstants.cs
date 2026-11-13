using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>Cartridge values and helpers of <see cref="BoyonAnimations"/> that only verification reads.</summary>
internal static class BoyonAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BoyonSegment[] programs) =>
        programs.SelectMany(program => BoyonAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, BoyonCue>.Frame>()
            .Select(frame => BoyonFrameToolingDefinitions.Spritemap(frame.Shown));
}
