using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Botwoon;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Botwoon colour document.</summary>
public static class BotwoonColorFile
{
    /// <summary>Loads and validates all eight complete Botwoon health palettes from editable RGB5 JSON.</summary>
    /// <param name="json">The caller-owned stream containing the color document.</param>
    /// <returns>The compiled immutable palette catalog.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, palette dimensions, or RGB5 channels are invalid.</exception>
    public static BotwoonColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        BotwoonColorDocument document;
        document = JsonAssetDocument.Read<BotwoonColorDocument>(json, ArtworkJson.Options, "Botwoon color");
        if (document.Version != BotwoonColorFormat.Version)
            throw new InvalidDataException("Botwoon colors require the supported version.");
        if (document.Health is null ||
            document.Health.Length != BotwoonHealthPaletteDefinitions.PaletteCount)
            throw new InvalidDataException("Botwoon colors require eight health bands.");
        return new(document.Health.Select((frame, index) =>
            Compile(frame, index)).ToArray());
    }

    /// <summary>Serializes a Botwoon color document and validates the resulting JSON through <see cref="Read"/>.</summary>
    /// <param name="document">The eight complete RGB5 health palettes to serialize.</param>
    /// <returns>Validated UTF-8 JSON bytes.</returns>
    public static byte[] Write(BotwoonColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Checks a health row's sixteen five-bit RGB values before packing its SNES colors.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int band)
    {
        if (source is null ||
            source.Length != BotwoonHealthPaletteDefinitions.ColorsPerPalette)
            throw new InvalidDataException(
                $"Botwoon health band {band} requires sixteen RGB5 colors.");
        var compiled = new RgbColor[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Botwoon health band {band} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToRgbColor();
        }
        return compiled;
    }
}
