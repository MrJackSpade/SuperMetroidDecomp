using System.Text.Json;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Installs the opening backdrops (#1544) in the opening cinematic's artwork: the solid colour the narration and the Ceres
/// scenes show where nothing is drawn, as one JSON document a player edits in place. The import takes each backdrop from
/// the first colour of the scene's own colours when it extracts them; the game reads these colours and neither palette
/// document, which only the import reads to pre-colour the opening's sheets.
/// </summary>
public static class IntroBackdropFiles
{
    /// <summary>The installed document's file name.</summary>
    public const string FileName = "intro-backdrops.json";

    /// <summary>Revision 1 of the backdrop document, the only revision the reader accepts.</summary>
    public const int Version = 1;

    /// <summary>The backdrops the scenes' extracted colours show, as the document's bytes.</summary>
    /// <param name="opening">The opening narration's extracted colours, whose first background colour is its backdrop.</param>
    /// <param name="flight">The Ceres flight's extracted colours, whose first colour is its backdrop.</param>
    internal static byte[] Extract(IntroCinematicPalette opening, CeresFlightPalette flight)
    {
        Ensure.NotNull(opening);
        Ensure.NotNull(flight);
        return JsonSerializer.SerializeToUtf8Bytes(new IntroBackdropDocument
        {
            Version = Version,
            Opening = PaletteRgb5.From(opening.Backdrop),
            CeresFlight = PaletteRgb5.From(flight.Backdrop),
        }, ArtworkJson.Options);
    }

    /// <summary>Reads the installed opening backdrops.</summary>
    /// <param name="json">The installed document, read from its current position.</param>
    /// <exception cref="InvalidDataException">
    /// The document is invalid or names another version, or a backdrop is missing or has a channel outside 0..31.
    /// </exception>
    public static IntroBackdrops Read(Stream json)
    {
        IntroBackdropDocument document = JsonAssetDocument.Read<IntroBackdropDocument>(json, ArtworkJson.Options, "opening backdrops");
        if (document.Version != Version)
            throw new InvalidDataException($"Opening backdrops require version {Version}.");
        return new IntroBackdrops(Colour(document.Opening, "opening backdrop"), Colour(document.CeresFlight, "Ceres flight backdrop"));

        static RgbColor Colour(PaletteRgb5? colour, string name) =>
            colour is { Red: >= 0 and <= FadeLevelColour.FullLevel, Green: >= 0 and <= FadeLevelColour.FullLevel, Blue: >= 0 and <= FadeLevelColour.FullLevel }
                ? colour.ToRgbColor()
                : throw new InvalidDataException($"Opening backdrops: the {name} requires red, green and blue in 0..{FadeLevelColour.FullLevel}.");
    }
}
