using System.Text.Json;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Reads and writes the installed Ceres Ridley Mode 7 colour document.</summary>
public static class CeresRidleyMode7ColorFile
{
    /// <summary>Loads all nine zoom shades, each containing fifteen RGB5 colors, rejecting duplicate/unknown JSON properties, unsupported versions, and channel values outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the editable Ceres getaway color document.</param>
    /// <returns>Installed zoom-dependent inks; the native sixteen-word row stride and unused padding are not editable content.</returns>
    /// <exception cref="InvalidDataException">The document schema, row dimensions, or RGB5 colors are invalid.</exception>
    public static CeresRidleyFloorColorCatalog Read(Stream json)
    {
        FixedGridShape zoomShape = new(CeresRidleyShadeDefinitions.GetawayZoomStepCount,
            CeresRidleyColorFormat.GetawayZoomColorCount);
        Ensure.NotNull(json);
        CeresRidleyMode7ColorDocument document;
        document = JsonAssetDocument.Read<CeresRidleyMode7ColorDocument>(json, ArtworkJson.Options, "Ceres Ridley Mode-7 color");
        if (document.Version != CeresRidleyFloorColorFormat.Version ||
            !zoomShape.HasRows(document.ZoomRows))
            throw new InvalidDataException("Ceres Ridley Mode-7 colors require version one and nine zoom rows.");
        var rows = new RgbColor[CeresRidleyShadeDefinitions.GetawayZoomStepCount][];
        for (int row = 0; row < rows.Length; row++)
        {
            PaletteRgb5[]? colors = document.ZoomRows[row];
            if (!zoomShape.HasColumns(colors))
                throw new InvalidDataException($"Ceres Ridley zoom row {row} requires fifteen colors.");
            rows[row] = new RgbColor[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Ceres Ridley zoom row {row} color {color} requires RGB5 channels 0..31.");
                rows[row][color] = rgb.ToRgbColor();
            }
        }
        return new(rows);
    }

    /// <summary>Serializes all zoom shades as UTF-8 JSON.</summary>
    /// <param name="document">Complete nine-row color document to serialize.</param>
    /// <returns>JSON bytes, without writing an external resource.</returns>
    public static byte[] Write(CeresRidleyMode7ColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
}
