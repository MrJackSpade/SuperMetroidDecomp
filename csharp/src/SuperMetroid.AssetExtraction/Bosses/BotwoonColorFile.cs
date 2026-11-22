using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Botwoon;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Botwoon colour document.</summary>
public static partial class BotwoonColorFile
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
        return new(Rgb5Rows.CompileKeyed(document.Health, BotwoonHealthBandDefinitions.InOrderShown, band => band.Key,
            BotwoonColorFormat.ColorsPerPalette, "Botwoon health band"));
    }

    /// <summary>Serializes a Botwoon color document as UTF-8 JSON.</summary>
    /// <param name="document">The eight complete RGB5 health palettes to serialize.</param>
    /// <returns>UTF-8 JSON bytes.</returns>
    public static byte[] Write(BotwoonColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Bosses.BotwoonColorDocument>(ArtworkJson.Options));
}
