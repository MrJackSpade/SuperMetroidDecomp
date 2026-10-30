using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The standard palettes frontend screens' pictures are composed under (#1392, #1467): the title's, an ending scene's and
/// the Ceres flight's, in the cartridge's colour order.
/// </summary>
internal static class ScreenPaletteColours
{

    /// <summary>The title's palette.</summary>
    internal static ReadOnlySpan<RgbColor> TitleColours(TitlePalettePresentation palette)
    {
        var colours = new ScreenPalette();
        Ensure.NotNull(palette).Apply(colours);
        return ScreenColourImage.Encode(colours);
    }

    /// <summary>The ending's palette <paramref name="palette"/> from <paramref name="palettes"/>.</summary>
    internal static ReadOnlySpan<RgbColor> EndingColours(EndingPaletteSet palettes, EndingPaletteId palette)
    {
        return ScreenColourImage.Encode(palettes[palette].Screen);
    }

    /// <summary>The Ceres flight's palette.</summary>
    internal static ReadOnlySpan<RgbColor> FlightColours(CeresFlightArtwork flight)
    {
        return ScreenColourImage.Encode(flight.Palette.Colours);
    }
}
