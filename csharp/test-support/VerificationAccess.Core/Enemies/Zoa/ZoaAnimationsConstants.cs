using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Zoa.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Cartridge values and helpers of <see cref="ZoaAnimations"/> that only verification reads.</summary>
internal static class ZoaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ZoaSegment[] programs) =>
        programs.SelectMany(program => ZoaAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, ZoaCue>.Frame>()
            .Select(frame => ZoaFrameToolingDefinitions.Spritemap(frame.Shown));
}
