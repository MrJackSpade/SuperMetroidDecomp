using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The standard palettes frontend screens' pictures are composed under (#1392, #1467): the title's, an ending scene's and
/// the Ceres flight's, in the cartridge's colour order.
/// </summary>
internal static class ScreenPaletteColours
{

    /// <summary>The title's palette.</summary>
    internal static ReadOnlySpan<RgbColor> TitleColours(TitlePalettePresentation palette) =>
        Ensure.NotNull(palette).Colours.AsSpan();

    /// <summary>The ending's palette <paramref name="palette"/> from <paramref name="palettes"/>.</summary>
    internal static ReadOnlySpan<RgbColor> EndingColours(EndingPaletteSet palettes, EndingPaletteId palette)
    {
        EndingPalette screen = Ensure.NotNull(palettes)[palette];
        return screen.IsScreen
            ? screen.Colours.AsSpan()
            : throw new ArgumentOutOfRangeException(nameof(palette), palette, "Not a whole ending screen's colours.");
    }

    /// <summary>The Ceres flight's palette.</summary>
    internal static ReadOnlySpan<RgbColor> FlightColours(CeresFlightArtwork flight) =>
        CeresFlightCharacters.Of(flight).Palette.Colours.AsSpan();
}
