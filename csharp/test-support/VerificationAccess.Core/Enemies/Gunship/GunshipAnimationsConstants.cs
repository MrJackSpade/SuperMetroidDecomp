using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Cartridge values and helpers of <see cref="GunshipAnimations"/> that only verification reads.</summary>
internal static class GunshipAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params GunshipSegment[] programs) =>
        programs.SelectMany(program => GunshipAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => GunshipFrameDefinitionsConstants.Spritemap(frame));
}
