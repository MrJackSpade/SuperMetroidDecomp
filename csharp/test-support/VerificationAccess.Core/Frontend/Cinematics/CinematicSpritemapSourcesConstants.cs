using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Cinematics;

/// <summary>Cartridge values and helpers of <see cref="CinematicSpritemapSources"/> that only verification reads.</summary>
internal static class CinematicSpritemapSourcesConstants
{
    /// <summary>
    /// Whether the cartridge's lists draw <paramref name="pose"/> when they select the bank-$8C spritemap at
    /// <paramref name="spritemap"/>: its own spritemap, or one of the further spritemaps that draw the same picture.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is the blank pose or is undefined.</exception>
    internal static bool IsDrawnBy(CinematicSpritePose pose, int spritemap) =>
        CinematicSpritemapSources.Of(pose) == spritemap
        || CinematicSpritemapSourceDefinitionsConstants.SamePictureSpritemaps.TryGetValue(pose, out SnesAddress[]? same)
            && same.Any(further => further.Offset == spritemap);
}
