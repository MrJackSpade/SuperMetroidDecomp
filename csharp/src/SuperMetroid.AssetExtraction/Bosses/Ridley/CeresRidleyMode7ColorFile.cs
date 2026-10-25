using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Reads and writes the installed Ceres Ridley Mode 7 colour document.</summary>
public static class CeresRidleyMode7ColorFile
{
    /// <summary>Loads all nine zoom shades, each containing fifteen RGB5 colors, rejecting duplicate/unknown JSON properties, unsupported versions, and channel values outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the editable Ceres getaway color document.</param>
    /// <returns>Installed zoom-dependent inks; the native sixteen-word row stride and unused padding are not editable content.</returns>
    /// <exception cref="InvalidDataException">The document schema, row dimensions, or RGB5 colors are invalid.</exception>
    public static CeresRidleyMode7ColorCatalog Read(Stream json)
    {
        FixedGridShape zoomShape = new(CeresRidleyPaletteDefinitions.GetawayZoomRowCount,
            CeresRidleyPaletteDefinitions.GetawayZoomColorCount);
        Ensure.NotNull(json);
        CeresRidleyMode7ColorDocument document;
        document = JsonAssetDocument.Read<CeresRidleyMode7ColorDocument>(json, ArtworkJson.Options, "Ceres Ridley Mode-7 color");
        if (document.Version != CeresRidleyMode7ColorFormat.Version ||
            !zoomShape.HasRows(document.ZoomRows))
            throw new InvalidDataException("Ceres Ridley Mode-7 colors require version one and nine zoom rows.");
        var rows = new Bgr555[CeresRidleyPaletteDefinitions.GetawayZoomRowCount][];
        for (int row = 0; row < rows.Length; row++)
        {
            PaletteRgb5[]? colors = document.ZoomRows[row];
            if (!zoomShape.HasColumns(colors))
                throw new InvalidDataException($"Ceres Ridley zoom row {row} requires fifteen colors.");
            rows[row] = new Bgr555[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Ceres Ridley zoom row {row} color {color} requires RGB5 channels 0..31.");
                rows[row][color] = rgb.ToBgr555();
            }
        }
        return new(rows);
    }

    /// <summary>Serializes all zoom shades as UTF-8 JSON and validates them through <see cref="Read"/> before returning the payload.</summary>
    /// <param name="document">Complete nine-row color document to validate and serialize.</param>
    /// <returns>Validated JSON bytes, without writing an external resource.</returns>
    public static byte[] Write(CeresRidleyMode7ColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));
}
